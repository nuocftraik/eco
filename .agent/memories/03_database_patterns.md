# ECO.WebApi - Database Patterns Memory

> 🧠 **Purpose**: Database design patterns and EF Core conventions
> 📅 **Last Updated**: 2026-01-28
> 👤 **Maintained By**: vuongnv1206

---

## Entity Framework Core Setup

### DbContext Structure
```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for aggregate roots only
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    // Don't add DbSet for child entities (OrderItem)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

---

## Entity Configuration Pattern

### Fluent API Configuration
**Each entity has separate configuration class**

```csharp
namespace ECO.WebApi.Infrastructure.Persistence.Configuration;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table name
        builder.ToTable("Products", "Catalog");

        // Primary key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Stock)
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
    }
}
```

---

## Auditable Fields Pattern

### AuditableEntity Configuration
```csharp
public class AuditableEntityConfig : IEntityTypeConfiguration<AuditableEntity>
{
    public void Configure(EntityTypeBuilder<AuditableEntity> builder)
    {
        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.CreatedOn)
            .IsRequired();

        builder.Property(e => e.LastModifiedBy)
            .IsRequired();

        builder.Property(e => e.LastModifiedOn);
    }
}
```

### Auto-set audit fields (SaveChangesAsync)
```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var userId = _currentUser.GetUserId();

    foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedBy = userId;
                entry.Entity.CreatedOn = DateTime.UtcNow;
                entry.Entity.LastModifiedBy = userId;
                entry.Entity.LastModifiedOn = DateTime.UtcNow;
                break;

            case EntityState.Modified:
                entry.Entity.LastModifiedBy = userId;
                entry.Entity.LastModifiedOn = DateTime.UtcNow;
                break;
        }
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

---

## Schema Organization

### Use schemas for logical grouping
```csharp
// Catalog schema
builder.ToTable("Products", "Catalog");
builder.ToTable("Categories", "Catalog");

// Ordering schema
builder.ToTable("Orders", "Ordering");
builder.ToTable("OrderItems", "Ordering");

// Identity schema
builder.ToTable("Users", "Identity");
builder.ToTable("Roles", "Identity");
```

---

## Relationships

### One-to-Many
```csharp
// Category has many Products
builder.HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```

### Many-to-Many (with junction entity)
```csharp
// Order - Product (through OrderItem)
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Order Order { get; private set; } = default!;
    public Product Product { get; private set; } = default!;
}

// Configuration
builder.HasOne(oi => oi.Order)
    .WithMany(o => o.OrderItems)
    .HasForeignKey(oi => oi.OrderId);

builder.HasOne(oi => oi.Product)
    .WithMany()
    .HasForeignKey(oi => oi.ProductId);
```

### Self-referencing
```csharp
// Category with parent-child
builder.HasOne(c => c.ParentCategory)
    .WithMany(c => c.SubCategories)
    .HasForeignKey(c => c.ParentCategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## Indexes

### Single column index
```csharp
builder.HasIndex(p => p.Name)
    .HasDatabaseName("IX_Products_Name");
```

### Composite index
```csharp
builder.HasIndex(p => new { p.CategoryId, p.Name })
    .HasDatabaseName("IX_Products_CategoryId_Name");
```

### Unique index
```csharp
builder.HasIndex(u => u.Email)
    .IsUnique()
    .HasDatabaseName("IX_Users_Email");
```

---

## Migration Patterns

### Migration Naming
```bash
# Pattern: {Verb}{Entity}{Action}
dotnet ef migrations add AddProductEntity
dotnet ef migrations add UpdateProductAddStockField
dotnet ef migrations add RemoveProductDescriptionField
dotnet ef migrations add CreateCategoryTable
```

### Migration Commands
```bash
# Create migration
cd src/Host/Host/
dotnet ef migrations add MigrationName \
  --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj

# Apply migration
dotnet ef database update \
  --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj

# Rollback to specific migration
dotnet ef database update PreviousMigrationName \
  --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj

# Remove last migration
dotnet ef migrations remove \
  --project ../../Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj
```

---

## Seeding Data

### Seed in ApplicationDbSeeder
```csharp
public class ApplicationDbSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task SeedDatabaseAsync(CancellationToken cancellationToken)
    {
        // 1. Seed master data (order matters!)
        await SeedActionsAsync();
        await SeedFunctionsAsync();
        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedPermissionsAsync();
        
        // 2. Run custom seeders
        await _customSeederRunner.RunSeedersAsync(cancellationToken);
    }
}
```

### Custom Seeder Pattern
```csharp
public class CategorySeeder : ICustomSeeder
{
    private readonly ApplicationDbContext _db;

    public CategorySeeder(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Check if already seeded
        if (await _db.Categories.AnyAsync(cancellationToken))
            return;

        var categories = new List<Category>
        {
            Category.Create("Electronics", "Electronic devices"),
            Category.Create("Clothing", "Apparel and accessories"),
            Category.Create("Books", "Books and publications")
        };

        await _db.Categories.AddRangeAsync(categories, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Query Patterns

### AsNoTracking for read-only queries
```csharp
✅ var products = await _db.Products
    .AsNoTracking()
    .Where(p => p.CategoryId == categoryId)
    .ToListAsync(cancellationToken);

❌ var products = await _db.Products
    .Where(p => p.CategoryId == categoryId)
    .ToListAsync(cancellationToken); // Will track entities
```

### Include for eager loading
```csharp
var product = await _db.Products
    .Include(p => p.Category)
    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
```

### Select for projections (better performance)
```csharp
✅ var productDtos = await _db.Products
    .Where(p => p.CategoryId == categoryId)
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        CategoryName = p.Category.Name
    })
    .ToListAsync(cancellationToken);

❌ var products = await _db.Products
    .Include(p => p.Category)
    .ToListAsync(cancellationToken);
```

---

## Specification Pattern

### Base Specification
```csharp
public class ProductByIdSpec : Specification<Product>
{
    public ProductByIdSpec(Guid id)
    {
        Query.Where(p => p.Id == id);
    }
}
```

### Complex Specification
```csharp
public class SearchProductsSpec : Specification<Product>
{
    public SearchProductsSpec(SearchProductsRequest request)
    {
        Query.Where(p => p.DeletedOn == null); // Soft delete filter

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Search(p => p.Name, $"%{request.Keyword}%");
        }

        if (request.CategoryId.HasValue)
        {
            Query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            Query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            Query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        // Include related data
        Query.Include(p => p.Category);

        // Ordering
        Query.OrderBy(p => p.Name);
    }
}
```

---

## Transaction Patterns

### Implicit transactions (SaveChanges)
```csharp
// Single SaveChanges = one transaction
var product = Product.Create(...);
await _repository.AddAsync(product, ct);
// Auto-committed
```

### Explicit transactions
```csharp
using var transaction = await _db.Database.BeginTransactionAsync(ct);
try
{
    // Multiple operations
    await _db.Products.AddAsync(product, ct);
    await _db.SaveChangesAsync(ct);
    
    await _db.Orders.AddAsync(order, ct);
    await _db.SaveChangesAsync(ct);
    
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

---

## Soft Delete Pattern

### Mark as deleted (don't actually delete)
```csharp
public class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime? DeletedOn { get; set; }
    public Guid? DeletedBy { get; set; }
}
```

### Global query filter
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply soft delete filter globally
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
        }
    }
}

private LambdaExpression BuildSoftDeleteFilter(Type entityType)
{
    var parameter = Expression.Parameter(entityType, "e");
    var property = Expression.Property(parameter, nameof(BaseEntity.DeletedOn));
    var condition = Expression.Equal(property, Expression.Constant(null));
    return Expression.Lambda(condition, parameter);
}
```

---

## Performance Best Practices

### ✅ DO:
1. Use AsNoTracking for read-only queries
2. Use Select projections instead of Include when possible
3. Use pagination for large datasets
4. Create indexes on foreign keys
5. Use async methods for all database operations
6. Batch multiple inserts with AddRange

### ❌ DON'T:
1. Use Include for DTOs (use Select instead)
2. Load entire tables (always filter)
3. Use lazy loading (prefer eager loading)
4. Call SaveChanges in loops
5. Return IQueryable from repository
6. Use EF Core for bulk operations (use SQL)

---

## Connection String Patterns

### Development
```json
{
  "ConnectionString": "Server=localhost;Database=ECODb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Production
```json
{
  "ConnectionString": "Server=prod-server;Database=ECODb;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Azure SQL
```json
{
  "ConnectionString": "Server=tcp:yourserver.database.windows.net,1433;Database=ECODb;User ID=admin;Password=${DB_PASSWORD};Encrypt=True;Connection Timeout=30;"
}
```

---

**Last Updated**: 2026-01-28
**Maintained By**: vuongnv1206
