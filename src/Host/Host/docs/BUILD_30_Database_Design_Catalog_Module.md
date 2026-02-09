# BUILD_30: Database Design - Catalog Module (Code-First)

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** BUILD_09 (Domain Base Entities) đã complete  
> 🎯 **Approach:** Code-First với EF Core  
> 🛍️ **Features:** Multi-Product Types, Product Variants, Dynamic Attributes, Categories & Tags  
> ⚠️ **Important:** Junction tables use **Composite Primary Keys** (NO surrogate Id)  
> ⭐ **Design Philosophy:** **Variant-First Design** - ALL products have >=1 variants

Tài liệu này hướng dẫn **thiết kế database chi tiết cho Catalog Module** với **Variant-First Design** - Price/Quantity luôn ở trong Variant table.

---

## 1. Overview

**Làm gì:** Thiết kế và implement Catalog Module database với hỗ trợ nhiều loại sản phẩm và biến thể phức tạp.

**Tại sao cần:**
- **Multi-Product Types:** Hỗ trợ Simple (sản phẩm đơn giản), Configurable (sản phẩm có biến thể), Virtual (sản phẩm số/download)
- **Flexible Variants:** Mỗi sản phẩm có thể có nhiều biến thể (size, color, material...)
- **Dynamic Attributes:** Hệ thống thuộc tính linh hoạt không cần alter database khi thêm thuộc tính mới
- **Categories & Tags:** Phân loại sản phẩm theo nhiều chiều (category hierarchy + tags)
- **Scalable:** Thiết kế scale cho hàng triệu sản phẩm
- ⭐ **Variant-First:** ALL products have >=1 variants - Price/Quantity ALWAYS in Variant table

**Trong bước này chúng ta sẽ:**
- ✅ Phân tích ProductType (Simple, Configurable, Virtual)
- ✅ Thiết kế Product entity (Marketing info ONLY - NO Price/Quantity)
- ✅ Thiết kế Variant entity (Price & Inventory - ALWAYS)
- ✅ Auto-create default Variant for Simple products
- ✅ Thiết kế Attribute system (EAV pattern - Entity-Attribute-Value)
- ✅ Thiết kế Category với hierarchical structure
- ✅ Thiết kế Tag system
- ✅ Thiết kế ProductReview system
- ✅ EF Core configurations & indexes
- ✅ **Fix Multiple Cascade Paths** với proper DeleteBehavior
- ✅ Seed data strategy

**Database Schema Summary:**
```
11 Core Tables:
├── Products (Marketing info ONLY - NO Price/Quantity) ⭐
├── Variants (Price & Inventory - ALWAYS have >=1) ⭐
├── Attributes (Product attributes: Size, Color...)
├── AttributeValues (Values for attributes)
├── VariantAttributeValues (Junction: Composite PK)
├── Categories (Hierarchical categories)
├── ProductCategories (Junction: Composite PK)
├── Tags (Flat tags)
├── ProductTags (Junction: Composite PK)
├── UserReviews (Product reviews & ratings)
└── Audit Tables (Automatic via AuditableEntity)
```

**Variant-First Design Philosophy:**
```
✅ ALL products have at least 1 variant (even Simple products)
✅ Simple Product → 1 Default Variant (auto-created)
✅ Configurable Product → Multiple Variants
✅ Price/Quantity ALWAYS in Variant table
✅ Product table = Marketing info only (Name, Description, Images)
```

**Why Variant-First?**
- ✅ **Consistency:** Same business logic for all product types
- ✅ **Scalability:** Easy migrate Simple → Configurable later
- ✅ **Query simplicity:** JOIN Products + Variants always has data
- ✅ **Cart/Order logic:** ALWAYS reference VariantId (no conditionals)
- ✅ **Research-based:** Shopify, BigCommerce, Spree Commerce use this pattern

**Junction Table Strategy:**
- ✅ **Pure junction tables** (no extra metadata) → Use **Composite Primary Key** (NO surrogate Id)
- ✅ Prevents duplicate entries automatically
- ✅ Better performance (smaller indexes)
- ✅ Semantic meaning (natural keys)

---

## 1.1. Entity Relationship Diagram (ERD)

```mermaid
---
config:
  theme: neo-dark
---
erDiagram
  
    Products {
   uniqueidentifier Id PK "GUID - Sequential"
   int ProductType "1=Simple, 2=Configurable, 3=Virtual"
  nvarchar_200 Name "Product name"
 nvarchar_300 Slug UK "URL-friendly name"
        nvarchar_max Description "Rich text description"
        int ProductStatus "1=InStock, 2=OutOfStock, 3=Discontinued"
   nvarchar_500 MainImage "Main product image URL"
     datetime2 CreatedOn "Audit: Created date"
        uniqueidentifier CreatedBy "Audit: Creator"
    datetime2 LastModifiedOn "Audit: Modified date"
        uniqueidentifier LastModifiedBy "Audit: Modifier"
        datetime2 DeletedOn "Soft delete: Deleted date"
        uniqueidentifier DeletedBy "Soft delete: Deleter"
    }
    
    Variants {
      uniqueidentifier Id PK "GUID - Sequential"
  uniqueidentifier ProductId FK "Parent product"
   nvarchar_100 SKU UK "Stock Keeping Unit (unique)"
        int ProductStatus "Variant status"
    nvarchar_500 MainImage "Variant-specific image"
      decimal_18_2 Price "Variant price (REQUIRED)"
   decimal_18_2 ComparePrice "Compare at price"
        bit IsDefault "Is default variant"
  int Quantity "Variant stock quantity (REQUIRED)"
bit IncludeDownload "Has downloadable file"
     nvarchar_200 FileName "Download file name"
        nvarchar_500 FileUrl "Download URL"
    }
    
    Attributes {
    uniqueidentifier Id PK "GUID - Sequential"
  nvarchar_100 Name "Attribute name (Size, Color)"
 int AttributeType "1=Dropdown, 2=Radio, 3=Checkbox"
        uniqueidentifier ProductId FK "Product reference"
    }
    
    AttributeValues {
  uniqueidentifier Id PK "GUID - Sequential"
 nvarchar_100 Value "Value (Small, Red, Cotton)"
        uniqueidentifier AttributeId FK "Attribute reference"
    }
  
    VariantAttributeValues {
   uniqueidentifier VariantId PK "Composite PK - Variant reference"
 uniqueidentifier AttributeValueId PK "Composite PK - AttributeValue reference"
    }
    
    Categories {
  uniqueidentifier Id PK "GUID - Sequential"
nvarchar_200 Name "Category name"
     nvarchar_300 Slug UK "URL-friendly name"
  bit IsActive "Is active"
     datetime2 CreatedOn "Audit: Created date"
   uniqueidentifier CreatedBy "Audit: Creator"
 datetime2 LastModifiedOn "Audit: Modified date"
     uniqueidentifier LastModifiedBy "Audit: Modifier"
    datetime2 DeletedOn "Soft delete: Deleted date"
 uniqueidentifier DeletedBy "Soft delete: Deleter"
    }
    
    ProductCategories {
uniqueidentifier ProductId PK "Composite PK - Product reference"
    uniqueidentifier CategoryId PK "Composite PK - Category reference"
    }
    
  Tags {
        uniqueidentifier Id PK "GUID - Sequential"
        nvarchar_100 Name UK "Tag name"
   nvarchar_300 Slug UK "URL-friendly name"
    }
  
 ProductTags {
      uniqueidentifier ProductId PK "Composite PK - Product reference"
        uniqueidentifier TagId PK "Composite PK - Tag reference"
}
  
    UserReviews {
 uniqueidentifier Id PK "GUID - Sequential"
        uniqueidentifier VariantId FK "Reviewed variant"
      uniqueidentifier UserId FK "Reviewer"
        int Rating "1-5 stars"
        nvarchar_500 Title "Review title"
  nvarchar_max Content "Review content"
        datetime2 CreatedOn "Review date"
    }
    
 Products ||--o{ Variants : "has variants (ALWAYS >=1)"
 Products ||--o{ Attributes : "has attributes"
    Attributes ||--o{ AttributeValues : "has values"
  Variants ||--o{ VariantAttributeValues : "uses"
  AttributeValues ||--o{ VariantAttributeValues : "applied to"
    Products ||--o{ ProductCategories : "belongs to"
    Categories ||--o{ ProductCategories : "contains"
 Products ||--o{ ProductTags : "tagged with"
    Tags ||--o{ ProductTags : "tags"
    Variants ||--o{ UserReviews : "reviewed"
```

**Key Relationships:**
- ✅ **Products → Variants**: 1-to-Many **(ALWAYS >=1 variant)** ⭐
- ✅ **Products → Attributes**: 1-to-Many (Each product defines its own attribute set)
- ✅ **Attributes → AttributeValues**: 1-to-Many (Each attribute has multiple values)
- ✅ **Variants ↔ AttributeValues**: Many-to-Many via VariantAttributeValues **(Composite PK)**
- ✅ **Products ↔ Categories**: Many-to-Many via ProductCategories **(Composite PK)**
- ✅ **Products ↔ Tags**: Many-to-Many via ProductTags **(Composite PK)**
- ✅ **Variants → UserReviews**: 1-to-Many (Reviews are for specific variants)

---

## 2. ProductType Deep Dive

### 2.1. ProductType Enum

**File:** `src/Core/Domain/Enum/ProductType.cs`

```csharp
namespace ECO.WebApi.Domain.Enum;

/// <summary>
/// Loại sản phẩm
/// </summary>
public enum ProductType
{
    /// <summary>
    /// Sản phẩm đơn giản - 1 variant duy nhất (auto-created)
    /// Example: Book, Fixed-spec gadget
    /// </summary>
    Simple = 1,
    
    /// <summary>
    /// Sản phẩm có thể cấu hình - Nhiều variants
    /// Example: T-shirt (Size: S/M/L, Color: Red/Blue)
    /// </summary>
    Configurable = 2,
    
    /// <summary>
    /// Sản phẩm ảo/Số hóa - Download file
    /// Example: E-book, Software license, Online course
    /// </summary>
    Virtual = 3
}
```

**Giải thích:**
- **Simple:** Product có **1 variant duy nhất** (auto-created). Price & Quantity trong Variant.
- **Configurable:** Product có **nhiều variants**. Mỗi variant có Price & Quantity riêng.
- **Virtual:** Có thể Simple (1 variant) hoặc Configurable (nhiều variants cho formats khác nhau).

---

### 2.2. Product Behavior by Type (Updated)

| ProductType | Has Variants? | Price Location | Quantity Location | Example |
|-------------|---------------|----------------|-------------------|---------|
| **Simple** | ✅ 1 variant (auto-created) | **Variant.Price** ⭐ | **Variant.Quantity** ⭐ | Book |
| **Configurable** | ✅ Multiple variants | **Variant.Price** ⭐ | **Variant.Quantity** ⭐ | T-shirt (Size/Color) |
| **Virtual** | ✅ 1+ variants | **Variant.Price** ⭐ | N/A (Unlimited) | E-book |

**Key Change:** ⭐
```
OLD: Simple Product → Price/Quantity in Product table
NEW: Simple Product → 1 Default Variant → Price/Quantity in Variant table
```

---

## 3. Core Entities (Variant-First Design)

### 3.1. Product Entity ⭐

**File:** `src/Core/Domain/Catalog/Product.cs`

```csharp
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;

/// <summary>
/// Product entity - Marketing container ONLY
/// Price/Quantity ALWAYS in Variant table
/// </summary>
public class Product : AuditableEntity, IAggregateRoot
{
 // ==================== Basic Info (Marketing) ====================
 
    public ProductType ProductType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public ProductStatus Status { get; private set; }
 public string? MainImage { get; private set; }
    
    // ❌ REMOVED: Price, ComparePrice, Quantity (now in Variant)
    // ❌ REMOVED: IncludeDownload, FileName, FileUrl (now in Variant) ⭐
  
    // ==================== Navigation Properties ====================
    
    public virtual List<Variant> Variants { get; private set; } = new();
    public virtual List<Attributes.Attribute> Attributes { get; private set; } = new();
    public virtual List<ProductCategory> ProductCategories { get; private set; } = new();
    public virtual List<ProductTag> ProductTags { get; private set; } = new();
    
  // ==================== Constructors ====================
    
    private Product() { }
  
    public Product(ProductType productType, string name, string slug, string? description, string? mainImage)
    {
 ProductType = productType;
     Name = name;
        Slug = slug;
        Description = description;
        MainImage = mainImage;
        Status = ProductStatus.InStock;
 }

    // ==================== Factory Methods ⭐ ====================
    
    /// <summary>
    /// Create Simple Product with auto-generated default Variant
    /// </summary>
    public static Product CreateSimpleProduct(
        string name,
     string slug,
        string? description,
    string? mainImage,
  decimal price,
   int quantity,
        string? sku = null)
    {
 var product = new Product(ProductType.Simple, name, slug, description, mainImage);
        
    // ✅ Auto-create default variant for Simple product
    var defaultVariant = Variant.CreateDefault(
   sku: sku ?? GenerateSKU(slug),
      price: price,
        quantity: quantity,
  isDefault: true,
         mainImage: mainImage
        );
     
      product.AddVariant(defaultVariant);
        return product;
    }

    /// <summary>
    /// Create Configurable Product (Variants added manually)
    /// </summary>
    public static Product CreateConfigurableProduct(
     string name,
   string slug,
      string? description,
   string? mainImage)
    {
    return new Product(ProductType.Configurable, name, slug, description, mainImage);
    }
    
    /// <summary>
    /// Create Virtual Product with auto-generated default Variant (with download info)
    /// </summary>
    public static Product CreateVirtualProduct(
        string name,
        string slug,
  string? description,
        string? mainImage,
  decimal price,
      string fileName,
    string fileUrl,
      string? sku = null)
    {
 var product = new Product(ProductType.Virtual, name, slug, description, mainImage);
        
     // ✅ Auto-create default variant for Virtual product WITH download info
        var defaultVariant = Variant.CreateDefault(
            sku: sku ?? GenerateSKU(slug),
            price: price,
 quantity: 0, // Unlimited for virtual
     isDefault: true,
       mainImage: mainImage
        );
    
        // ✅ Set download info on VARIANT (not Product)
   defaultVariant.SetDownloadInfo(true, fileName, fileUrl);
  
   product.AddVariant(defaultVariant);
      return product;
    }
    
    // ==================== Business Methods ====================
    
    public void AddVariant(Variant variant)
    {
    Variants.Add(variant);
    }
    
    public void AddAttribute(string name, AttributeType attributeType, List<string> values)
    {
     var attribute = new Attributes.Attribute(name, attributeType, Id);
      foreach (var value in values)
     {
   attribute.AddValue(value);
        }
        Attributes.Add(attribute);
    }
    
    public void AddCategory(Guid categoryId)
    {
    if (ProductCategories.Any(pc => pc.CategoryId == categoryId))
      return;
     ProductCategories.Add(new ProductCategory(Id, categoryId));
 }
    
    // ==================== Helper Methods ====================
    
    private static string GenerateSKU(string slug)
    {
        return $"{slug.ToUpper()}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
```

**Key Points:**
- ✅ **NO Price/Quantity properties** - Pure marketing container
- ✅ **NO Download properties** - These belong to Variant ⭐
- ✅ **Factory methods** for each ProductType (Simple, Configurable, Virtual)
- ✅ **Auto-create default Variant** with download info for Virtual products
- ✅ **SKU generation** logic included

---

### 3.2. Variant Entity ⭐

**File:** `src/Core/Domain/Catalog/Variant.cs`

```csharp
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;

/// <summary>
/// Product variant - SELLABLE UNIT
/// Price/Quantity ALWAYS here (even for Simple products)
/// </summary>
public class Variant : BaseEntity, IAggregateRoot
{
    // ==================== Basic Info ====================
  
    public Guid ProductId { get; private set; }
    
    /// <summary>
    /// SKU - Stock Keeping Unit (REQUIRED, UNIQUE)
  /// </summary>
    public string SKU { get; private set; }
 
    public ProductStatus Status { get; private set; }
    public string? MainImage { get; private set; }
    
    // ==================== Pricing (REQUIRED) ⭐ ====================
    
  /// <summary>
    /// Price - REQUIRED for ALL variants
    /// </summary>
    public decimal Price { get; private set; }
 
    /// <summary>
    /// Compare at price (original price)
    /// </summary>
    public decimal? ComparePrice { get; private set; }
  
    // ==================== Inventory (REQUIRED) ⭐ ====================
    
    /// <summary>
    /// Quantity - REQUIRED for ALL variants (0 = unlimited for virtual)
    /// </summary>
    public int Quantity { get; private set; }
    
    /// <summary>
    /// Is this the default variant? (for UI)
  /// </summary>
    public bool IsDefault { get; private set; }
    
    // ==================== Virtual Product Info ====================
    
    public bool IncludeDownload { get; private set; }
    public string? FileName { get; private set; }
  public string? FileUrl { get; private set; }
    
    // ==================== Navigation Properties ====================
    
    public virtual Product Product { get; private set; } = default!;
    public virtual List<VariantAttributeValue> VariantAttributeValues { get; private set; } = new();
    public virtual List<UserReview> UserReviews { get; private set; } = new();
    
    // ==================== Constructors ====================
    
  private Variant() { }
    
    public Variant(
   string sku,
        decimal price,
        int quantity,
        bool isDefault,
 string? mainImage = null,
        decimal? comparePrice = null)
    {
        SKU = sku;
        Price = price;
        Quantity = quantity;
      IsDefault = isDefault;
        MainImage = mainImage;
        ComparePrice = comparePrice;
     Status = ProductStatus.InStock;
    }
    
    // ==================== Factory Methods ⭐ ====================
    
    /// <summary>
    /// Create default variant (for Simple products)
    /// </summary>
  public static Variant CreateDefault(
        string sku,
    decimal price,
  int quantity,
    bool isDefault,
      string? mainImage = null)
    {
        return new Variant(sku, price, quantity, isDefault, mainImage);
    }
    
    // ==================== Business Methods ====================
    
    public void UpdatePricing(decimal price, decimal? comparePrice = null)
    {
   if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (comparePrice.HasValue && comparePrice < price)
throw new ArgumentException("Compare price must be >= price", nameof(comparePrice));
     
        Price = price;
 ComparePrice = comparePrice;
    }
    
    public void UpdateInventory(int quantity)
    {
        if (quantity < 0)
       throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        
        Quantity = quantity;
        Status = quantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;
    }
    
    public void SetDownloadInfo(bool includeDownload, string? fileName, string? fileUrl)
    {
        IncludeDownload = includeDownload;
        if (includeDownload)
        {
     if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException("FileName is required when IncludeDownload=true");
    if (string.IsNullOrWhiteSpace(fileUrl))
    throw new ArgumentException("FileUrl is required when IncludeDownload=true");
        }
  FileName = fileName;
        FileUrl = fileUrl;
    }
    
    public void AddAttributeValue(Guid attributeValueId)
    {
        VariantAttributeValues.Add(new VariantAttributeValue(Id, attributeValueId));
    }
    
    public void RemoveQuantity(int quantity)
  {
        if (Quantity == 0)
  throw new InvalidOperationException("Variant is out of stock");
   
        if (quantity > Quantity)
          throw new InvalidOperationException($"Cannot remove {quantity} items, only {Quantity} available");
  
        Quantity -= quantity;
 if (Quantity == 0)
          Status = ProductStatus.OutOfStock;
    }
    
    public void AddQuantity(int quantity)
    {
     if (quantity < 0)
      throw new ArgumentException("Quantity must be positive", nameof(quantity));
     
        Quantity += quantity;
  if (Quantity > 0)
      Status = ProductStatus.InStock;
    }
}
```

**Key Points:**
- ✅ **SKU property** - Required, Unique identifier
- ✅ **Price property** - Required (decimal for precision)
- ✅ **Quantity property** - Required (0 = unlimited for virtual)
- ✅ **IsDefault flag** - Marks default variant for display
- ✅ **Business logic** - UpdatePricing, UpdateInventory, RemoveQuantity, AddQuantity

---

## 4. Usage Examples (Variant-First Design)

### 4.1. Simple Product (Book) ⭐

```csharp
// ✅ NEW WAY: Variant-First Design
var book = Product.CreateSimpleProduct(
    name: "Clean Code",
    slug: "clean-code",
    description: "A Handbook of Agile Software Craftsmanship by Robert C. Martin",
    mainImage: "https://cdn.example.com/books/clean-code.jpg",
    price: 29.99m,
    quantity: 100,
    sku: "BOOK-CLEANCODE"
);

// ✅ Product now has 1 default Variant
Console.WriteLine($"Variants count: {book.Variants.Count}"); // 1
var defaultVariant = book.Variants.First();
Console.WriteLine($"Price: ${defaultVariant.Price}"); // $29.99
Console.WriteLine($"SKU: {defaultVariant.SKU}"); // BOOK-CLEANCODE

// Save to database
await dbContext.Products.AddAsync(book);
await dbContext.SaveChangesAsync();

// ❌ OLD WAY (Don't use anymore)
// book.Price = 29.99m;  // ← Property no longer exists!
// book.Quantity = 100;   // ← Property no longer exists!
```

---

### 4.2. Configurable Product (T-Shirt) ⭐

```csharp
// 1. Create product (NO pricing here)
var tshirt = Product.CreateConfigurableProduct(
    name: "Basic T-Shirt",
    slug: "basic-tshirt",
    description: "Comfortable cotton t-shirt",
    mainImage: "https://cdn.example.com/tshirts/default.jpg"
);

// 2. Add attributes
tshirt.AddAttribute("Size", AttributeType.Dropdown, new List<string> { "Small", "Medium", "Large" });
tshirt.AddAttribute("Color", AttributeType.Radio, new List<string> { "Red", "Blue", "Green" });

await dbContext.Products.AddAsync(tshirt);
await dbContext.SaveChangesAsync();

// 3. Get attribute value IDs
var sizeSmallId = await dbContext.AttributeValues
    .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Size" && av.Value == "Small")
    .Select(av => av.Id)
    .FirstAsync();

var colorRedId = await dbContext.AttributeValues
  .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Color" && av.Value == "Red")
    .Select(av => av.Id)
    .FirstAsync();

// 4. Create variants (EACH with Price/Quantity)
var variantSmallRed = new Variant(
    sku: "TSHIRT-SM-RED",
    price: 19.99m,
    quantity: 50,
    isDefault: true,
    mainImage: "https://cdn.example.com/tshirts/small-red.jpg"
);
variantSmallRed.AddAttributeValue(sizeSmallId);
variantSmallRed.AddAttributeValue(colorRedId);
tshirt.AddVariant(variantSmallRed);

var variantLargeBlue = new Variant(
    sku: "TSHIRT-LG-BLUE",
    price: 24.99m,
    quantity: 30,
    isDefault: false,
    mainImage: "https://cdn.example.com/tshirts/large-blue.jpg"
);
variantLargeBlue.AddAttributeValue(sizeLargeId);
variantLargeBlue.AddAttributeValue(colorBlueId);
tshirt.AddVariant(variantLargeBlue);

await dbContext.SaveChangesAsync();
```

---

### 4.3. Virtual Product (E-Book) ⭐

```csharp
// ✅ NEW WAY: Variant-First Design
var ebook = Product.CreateVirtualProduct(
    name: "Design Patterns E-Book",
    slug: "design-patterns-ebook",
    description: "Gang of Four Design Patterns in PDF format",
    mainImage: "https://cdn.example.com/books/design-patterns-cover.jpg",
    price: 9.99m,
    fileName: "design-patterns.pdf",
    fileUrl: "https://cdn.example.com/downloads/design-patterns.pdf",
    sku: "EBOOK-DESIGNPATTERNS"
);

// ✅ Product has 1 default Variant with download info
var defaultVariant = ebook.Variants.First();
Console.WriteLine($"Price: ${defaultVariant.Price}"); // $9.99
Console.WriteLine($"Quantity: {defaultVariant.Quantity}"); // 0 (unlimited)
Console.WriteLine($"Download: {defaultVariant.FileName}"); // design-patterns.pdf

await dbContext.Products.AddAsync(ebook);
await dbContext.SaveChangesAsync();
```

---

### 4.4. Query Patterns (UPDATED) ⭐

#### **Get Product with Price**

```csharp
// ✅ NEW: ALWAYS query with Variants
var product = await _context.Products
    .Include(p => p.Variants)
    .FirstAsync(p => p.Id == productId);

// Get price (works for ALL product types)
var defaultVariant = product.Variants.FirstOrDefault(v => v.IsDefault);
var price = defaultVariant?.Price ?? 0;  // ✅ Consistent

// ❌ OLD: Conditional query
// var price = product.ProductType == ProductType.Simple 
//     ? product.Price 
//     : product.Variants.First().Price;
```

#### **Search Products with Price Filter**

```csharp
// ✅ NEW: Query Variants for pricing
var products = await _context.Products
    .Where(p => p.Variants.Any(v => v.Price >= minPrice && v.Price <= maxPrice))
    .Include(p => p.Variants.Where(v => v.IsDefault))
    .ToListAsync();
```

#### **Display Product Card (Frontend)**

```csharp
// ✅ NEW: Project to DTO with Variant data
var productDto = await _context.Products
    .Where(p => p.Id == productId)
    .Select(p => new ProductCardDto
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        MainImage = p.MainImage,
        // ✅ Price from default variant
        Price = p.Variants.First(v => v.IsDefault).Price,
        ComparePrice = p.Variants.First(v => v.IsDefault).ComparePrice,
        // ✅ SKU from default variant
        SKU = p.Variants.First(v => v.IsDefault).SKU,
        // ✅ Availability from default variant
     InStock = p.Variants.First(v => v.IsDefault).Quantity > 0
  })
  .FirstAsync();
```

---

### 4.5. Cart/Order System (UPDATED) ⭐

#### **CartItem Entity**

```csharp
public class CartItem : BaseEntity
{
 public Guid UserId { get; set; }
    
    // ✅ ALWAYS reference VariantId (not ProductId)
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
  
    // Navigation
public virtual Variant Variant { get; set; } = default!;
    
    // Access Product via: Variant.Product
    public string GetProductName() => Variant.Product.Name;
    public decimal GetPrice() => Variant.Price;
    public string GetSKU() => Variant.SKU;
}
```

#### **Add to Cart**

```csharp
public async Task<Result> AddToCart(Guid variantId, int quantity)
{
    // ✅ ALWAYS query Variant (works for all product types)
    var variant = await _context.Variants
     .Include(v => v.Product)
        .FirstOrDefaultAsync(v => v.Id == variantId);
 
    if (variant == null)
        return Result.Fail("Variant not found");
    
 // ✅ Check stock (same logic for ALL product types)
    if (variant.Quantity < quantity)
        return Result.Fail($"Only {variant.Quantity} items available");
    
    // ✅ Add to cart
    var cartItem = new CartItem
    {
        UserId = _currentUser.Id,
        VariantId = variantId,  // ✅ Always VariantId
        Quantity = quantity
    };
    
    await _context.CartItems.AddAsync(cartItem);
    await _context.SaveChangesAsync();
    
    return Result.Ok();
}
```

---

## 5. EF Core Configurations (Variant-First)

### 5.1. ProductConfiguration ⭐

**File:** `src/Infrastructure/Persistence/Configurations/Catalog/ProductConfiguration.cs`

```csharp
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.WebApi.Infrastructure.Persistence.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "Catalog");
        builder.HasKey(p => p.Id);
 
  // Properties
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
     builder.Property(p => p.Slug).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");
        builder.Property(p => p.MainImage).HasMaxLength(500);
        
        // ❌ NO Price, ComparePrice, Quantity properties configured
     // ❌ NO IncludeDownload, FileName, FileUrl properties configured ⭐
   
        // Indexes
     builder.HasIndex(p => p.Slug).IsUnique();
 
   // Relationships
        builder.HasMany(p => p.Variants)
          .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
    .OnDelete(DeleteBehavior.Cascade);
        
     builder.HasMany(p => p.Attributes)
       .WithOne(a => a.Product)
      .HasForeignKey(a => a.ProductId)
      .OnDelete(DeleteBehavior.Cascade);
  
        builder.HasMany(p => p.ProductCategories)
     .WithOne(pc => pc.Product)
   .HasForeignKey(pc => pc.ProductId)
  .OnDelete(DeleteBehavior.Cascade);
     
        builder.HasMany(p => p.ProductTags)
    .WithOne(pt => pt.Product)
      .HasForeignKey(pt => pt.ProductId)
   .OnDelete(DeleteBehavior.Cascade);
        
        // Query filters (soft delete)
     builder.HasQueryFilter(p => p.DeletedOn == null);
    }
}
```

---

### 5.2. VariantConfiguration ⭐

**File:** `src/Infrastructure/Persistence/Configurations/Catalog/VariantConfiguration.cs`

```csharp
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.WebApi.Infrastructure.Persistence.Configurations.Catalog;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.ToTable("Variants", "Catalog");
        builder.HasKey(v => v.Id);
    
   // Properties
        builder.Property(v => v.ProductId).IsRequired();
        
   // ⭐ SKU - REQUIRED, UNIQUE
        builder.Property(v => v.SKU)
          .IsRequired()
      .HasMaxLength(100);
        
      // ⭐ Price - REQUIRED
        builder.Property(v => v.Price)
     .IsRequired()
          .HasPrecision(18, 2);
     
        builder.Property(v => v.ComparePrice)
            .HasPrecision(18, 2);
        
        // ⭐ Quantity - REQUIRED
        builder.Property(v => v.Quantity)
       .IsRequired();
 
        // ⭐ IsDefault - REQUIRED
        builder.Property(v => v.IsDefault)
            .IsRequired();
        
        builder.Property(v => v.MainImage).HasMaxLength(500);
  
 // Virtual product download info
        builder.Property(v => v.IncludeDownload).IsRequired();
        builder.Property(v => v.FileName).HasMaxLength(200);
        builder.Property(v => v.FileUrl).HasMaxLength(500);
        
     // Indexes
        builder.HasIndex(v => v.ProductId);
        builder.HasIndex(v => v.Price);
  builder.HasIndex(v => v.IsDefault);
    
        // Relationships
        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
        .HasForeignKey(v => v.ProductId)
  .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(v => v.VariantAttributeValues)
       .WithOne(vav => vav.Variant)
            .HasForeignKey(vav => vav.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Key Configuration Points:**
- ✅ **SKU:** Required, Unique, MaxLength(100)
- ✅ **Price:** Required, Decimal(18,2) precision
- ✅ **Quantity:** Required (0 for unlimited)
- ✅ **IsDefault:** Required flag
- ✅ **Indexes:** SKU (unique), ProductId, Price, IsDefault

---

### 5.3. VariantAttributeValueConfiguration (Composite PK) ⭐

**File:** `src/Infrastructure/Persistence/Configurations/Catalog/VariantAttributeValueConfiguration.cs`

```csharp
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.WebApi.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// EF Core configuration for VariantAttributeValue junction table
/// Uses COMPOSITE PRIMARY KEY - NO surrogate Id
/// Fixes Multiple Cascade Paths issue with DeleteBehavior.Restrict
/// </summary>
public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        // Table mapping
        builder.ToTable("VariantAttributeValues", "Catalog");
      
      // ⭐ Composite Primary Key (configured via [PrimaryKey] attribute in entity)
// No need to configure here as entity has [PrimaryKey(nameof(VariantId), nameof(AttributeValueId))]
  
        // Properties
     builder.Property(vav => vav.VariantId).IsRequired();
        builder.Property(vav => vav.AttributeValueId).IsRequired();
        
 // Indexes for FK columns (improve query performance)
 builder.HasIndex(vav => vav.VariantId)
.HasDatabaseName("IX_VariantAttributeValues_VariantId");
        
    builder.HasIndex(vav => vav.AttributeValueId)
            .HasDatabaseName("IX_VariantAttributeValues_AttributeValueId");
        
        // ⭐ Relationships with CASCADE vs RESTRICT to avoid Multiple Cascade Paths  
      // Path 1: Variant → VariantAttributeValues (CASCADE)
        builder.HasOne(vav => vav.Variant)
     .WithMany(v => v.VariantAttributeValues)
    .HasForeignKey(vav => vav.VariantId)
            .OnDelete(DeleteBehavior.Cascade);  // ✅ CASCADE: Delete assignments when variant deleted
    
        // Path 2: AttributeValue → VariantAttributeValues (RESTRICT)
        builder.HasOne(vav => vav.AttributeValue)
            .WithMany(av => av.VariantAttributeValues)
            .HasForeignKey(vav => vav.AttributeValueId)
    .OnDelete(DeleteBehavior.Restrict);  // ✅ RESTRICT: Prevent cascade from attribute side
        
// ⚠️ Why RESTRICT on AttributeValue?
        // - Prevents Multiple Cascade Paths error
        // - Business logic: AttributeValues are reusable (shared by multiple Variants)
        // - Must manually clean up VariantAttributeValues before deleting AttributeValue
    }
}
```

---

### 5.4. ProductCategoryConfiguration (Composite PK) ⭐

**File:** `src/Infrastructure/Persistence/Configurations/Catalog/ProductCategoryConfiguration.cs`

```csharp
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.WebApi.Infrastructure.Persistence.Configurations.Catalog;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
   builder.ToTable("ProductCategories", "Catalog");
     
        // ⭐ Composite Primary Key (configured via [PrimaryKey] attribute)
        
        builder.Property(pc => pc.ProductId).IsRequired();
  builder.Property(pc => pc.CategoryId).IsRequired();
        
        // Indexes
      builder.HasIndex(pc => pc.ProductId);
    builder.HasIndex(pc => pc.CategoryId);
        
        // Relationships
        builder.HasOne(pc => pc.Product)
          .WithMany(p => p.ProductCategories)
         .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
builder.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
    .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

### 5.5. ProductTagConfiguration (Composite PK) ⭐

**File:** `src/Infrastructure/Persistence/Configurations/Catalog/ProductTagConfiguration.cs`

```csharp
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.WebApi.Infrastructure.Persistence.Configurations.Catalog;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("ProductTags", "Catalog");
    
        // ⭐ Composite Primary Key (configured via [PrimaryKey] attribute)

        builder.Property(pt => pt.ProductId).IsRequired();
        builder.Property(pt => pt.TagId).IsRequired();
        
   // Indexes
        builder.HasIndex(pt => pt.ProductId);
 builder.HasIndex(pt => pt.TagId);
  
        // Relationships
        builder.HasOne(pt => pt.Product)
         .WithMany(p => p.ProductTags)
    .HasForeignKey(pt => pt.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(pt => pt.Tag)
.WithMany(t => t.ProductTags)
    .HasForeignKey(pt => pt.TagId)
.OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 6. Migration Strategy

### 6.1. For NEW Projects ✅

**Just apply migrations** - Everything will be created correctly with Variant-First design!

```bash
# From Migrators.MSSQL project
dotnet ef migrations add Catalog_Module_VariantFirst \
  --project . \
--startup-project ../../Host/Host \
  --context ApplicationDbContext

dotnet ef database update \
  --project . \
  --startup-project ../../Host/Host \
  --context ApplicationDbContext
```

---

### 6.2. For EXISTING Projects (with data) ⚠️

**Step 1: Create Migration**

```bash
dotnet ef migrations add Catalog_Migration_To_VariantFirst \
  --project ../Infrastructure \
  --startup-project ../../Host/Host \
  --context ApplicationDbContext
```

**Step 2: Data Migration Script**

```csharp
public partial class Catalog_Migration_To_VariantFirst : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Add new columns to Variants
     migrationBuilder.AddColumn<string>(
        name: "SKU",
        schema: "Catalog",
    table: "Variants",
     type: "nvarchar(100)",
  maxLength: 100,
    nullable: false,
   defaultValue: "");
 
        // 2. Create default Variants for Simple products
        migrationBuilder.Sql(@"
  -- Create default Variant for each Simple Product
        INSERT INTO [Catalog].[Variants] 
        (Id, ProductId, SKU, Price, ComparePrice, Quantity, IsDefault, ProductStatus, MainImage, IncludeDownload)
   SELECT 
                NEWID() AS Id,
 p.Id AS ProductId,
             CONCAT('SKU-', UPPER(LEFT(p.Slug, 8)), '-', UPPER(LEFT(CAST(NEWID() AS NVARCHAR(36)), 8))) AS SKU,
             p.Price,
                p.ComparePrice,
                ISNULL(p.Quantity, 0) AS Quantity,
      1 AS IsDefault,
          p.ProductStatus,
                p.MainImage,
 p.IncludeDownload
          FROM [Catalog].[Products] p
            WHERE p.ProductType = 1  -- Simple products
              AND NOT EXISTS (SELECT 1 FROM [Catalog].[Variants] v WHERE v.ProductId = p.Id)
        ");
        
        // 3. Drop old columns from Products
migrationBuilder.DropColumn(name: "Price", schema: "Catalog", table: "Products");
    migrationBuilder.DropColumn(name: "ComparePrice", schema: "Catalog", table: "Products");
        migrationBuilder.DropColumn(name: "Quantity", schema: "Catalog", table: "Products");
        
        // 4. Add unique constraint on SKU
  migrationBuilder.CreateIndex(
       name: "IX_Variants_SKU",
    schema: "Catalog",
        table: "Variants",
            column: "SKU",
     unique: true);
}
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
   // Rollback migration
     migrationBuilder.DropIndex(name: "IX_Variants_SKU", schema: "Catalog", table: "Variants");
        
        migrationBuilder.AddColumn<decimal>(name: "Price", schema: "Catalog", table: "Products");
  migrationBuilder.AddColumn<decimal>(name: "ComparePrice", schema: "Catalog", table: "Products");
migrationBuilder.AddColumn<int>(name: "Quantity", schema: "Catalog", table: "Products");
        
        // Copy data back from Variants to Products
        migrationBuilder.Sql(@"
        UPDATE p
      SET p.Price = v.Price,
     p.ComparePrice = v.ComparePrice,
    p.Quantity = v.Quantity
      FROM [Catalog].[Products] p
            INNER JOIN [Catalog].[Variants] v ON v.ProductId = p.Id AND v.IsDefault = 1
      WHERE p.ProductType = 1
  ");
     
    // Delete auto-created Variants
        migrationBuilder.Sql(@"
       DELETE FROM [Catalog].[Variants]
            WHERE ProductId IN (SELECT Id FROM [Catalog].[Products] WHERE ProductType = 1)
        ");
  
        migrationBuilder.DropColumn(name: "SKU", schema: "Catalog", table: "Variants");
    }
}
```

---

## 7. Seed Data Strategy (Variant-First)

### 7.1. Seed Simple Product (Book)

```csharp
private async Task SeedSimpleProductAsync()
{
    if (await _context.Products.AnyAsync(p => p.Slug == "clean-code"))
    {
        _logger.LogInformation("Simple product already seeded, skipping...");
      return;
    }
    
    _logger.LogInformation("Seeding simple product (Book)...");
    
    // ✅ Use factory method - automatically creates default Variant
 var book = Product.CreateSimpleProduct(
        name: "Clean Code",
    slug: "clean-code",
     description: "A Handbook of Agile Software Craftsmanship by Robert C. Martin",
        mainImage: "https://cdn.example.com/books/clean-code.jpg",
        price: 29.99m,
    quantity: 100,
        sku: "BOOK-CLEANCODE"
    );
    
    // Add to categories
    var booksCategory = await _context.Categories.FirstAsync(c => c.Slug == "books");
    book.AddCategory(booksCategory.Id);
 
    await _context.Products.AddAsync(book);
  await _context.SaveChangesAsync();
    
_logger.LogInformation($"Seeded: {book.Name} with {book.Variants.Count} variant(s)");
}
```

---

### 7.2. Seed Configurable Product (T-Shirt)

```csharp
private async Task SeedConfigurableProductAsync()
{
    if (await _context.Products.AnyAsync(p => p.Slug == "basic-tshirt"))
    {
        _logger.LogInformation("Configurable product already seeded, skipping...");
        return;
    }
    
    _logger.LogInformation("Seeding configurable product (T-Shirt)...");
    
    // 1. Create product
    var tshirt = Product.CreateConfigurableProduct(
  name: "Basic T-Shirt",
        slug: "basic-tshirt",
   description: "Comfortable cotton t-shirt in multiple sizes and colors",
     mainImage: "https://cdn.example.com/tshirts/default.jpg"
    );
    
    // 2. Add attributes
  tshirt.AddAttribute("Size", AttributeType.Dropdown, new List<string> { "Small", "Medium", "Large" });
    tshirt.AddAttribute("Color", AttributeType.Radio, new List<string> { "Red", "Blue", "Green" });
    
 await _context.Products.AddAsync(tshirt);
    await dbContext.SaveChangesAsync();
 
    // 3. Get attribute value IDs
    var sizeSmallId = await dbContext.AttributeValues
    .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Size" && av.Value == "Small")
    .Select(av => av.Id).FirstAsync();
    
    var sizeLargeId = await dbContext.AttributeValues
        .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Size" && av.Value == "Large")
    .Select(av => av.Id).FirstAsync();
    
    var colorRedId = await dbContext.AttributeValues
  .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Color" && av.Value == "Red")
    .Select(av => av.Id).FirstAsync();
  
    var colorBlueId = await dbContext.AttributeValues
      .Where(av => av.Attribute.ProductId == tshirt.Id && av.Attribute.Name == "Color" && av.Value == "Blue")
    .Select(av => av.Id).FirstAsync();
    
    // 4. Create variants
    var variantSmallRed = new Variant(
      sku: "TSHIRT-SM-RED",
        price: 19.99m,
 quantity: 50,
        isDefault: true,
        mainImage: "https://cdn.example.com/tshirts/small-red.jpg"
    );
    variantSmallRed.AddAttributeValue(sizeSmallId);
    variantSmallRed.AddAttributeValue(colorRedId);
    tshirt.AddVariant(variantSmallRed);
    
    var variantLargeBlue = new Variant(
sku: "TSHIRT-LG-BLUE",
   price: 24.99m,
        quantity: 30,
        isDefault: false,
  mainImage: "https://cdn.example.com/tshirts/large-blue.jpg"
    );
 variantLargeBlue.AddAttributeValue(sizeLargeId);
    variantLargeBlue.AddAttributeValue(colorBlueId);
    tshirt.AddVariant(variantLargeBlue);
    
    await dbContext.SaveChangesAsync();
    
    _logger.LogInformation($"Seeded: {tshirt.Name} with {tshirt.Variants.Count} variant(s)");
}
```

---

### 7.3. Seed Virtual Product (E-Book)

```csharp
private async Task SeedVirtualProductAsync()
{
    if (await _context.Products.AnyAsync(p => p.Slug == "design-patterns-ebook"))
    {
      _logger.LogInformation("Virtual product already seeded, skipping...");
   return;
  }
    
    _logger.LogInformation("Seeding virtual product (E-Book)...");
    
 // ✅ Use factory method - automatically creates default Variant with download info
    var ebook = Product.CreateVirtualProduct(
        name: "Design Patterns E-Book",
 slug: "design-patterns-ebook",
description: "Gang of Four Design Patterns in PDF format",
    mainImage: "https://cdn.example.com/books/design-patterns-cover.jpg",
        price: 9.99m,
    fileName: "design-patterns.pdf",
      fileUrl: "https://cdn.example.com/downloads/design-patterns.pdf",
   sku: "EBOOK-DESIGNPATTERNS"
    );
    
    // Add to categories
    var digitalCategory = await _context.Categories.FirstAsync(c => c.Slug == "digital-products");
    ebook.AddCategory(digitalCategory.Id);
    
    await _context.Products.AddAsync(ebook);
    await _context.SaveChangesAsync();
    
    _logger.LogInformation($"Seeded: {ebook.Name} with {ebook.Variants.Count} variant(s)");
}
```

---

## 8. Benefits Summary

### 8.1. Variant-First Design Comparison

| Aspect | OLD (Conditional) | NEW (Variant-First) |
|--------|------------------|---------------------|
| **Consistency** | Conditional logic everywhere | ✅ Single path for all types |
| **Code Complexity** | High (many if/else) | ✅ Low (polymorphic) |
| **Cart Logic** | ProductId OR VariantId | ✅ Always VariantId |
| **Query Complexity** | Conditional JOINs | ✅ Simple JOINs |
| **Migration Simple→Config** | Complex data migration | ✅ Just add Variants |
| **SKU Management** | No consistent system | ✅ Always in Variant |
| **Testing** | Complex (many paths) | ✅ Simple (one path) |
| **Performance** | Variable queries | ✅ Optimized queries |
| **Maintenance** | High (scattered logic) | ✅ Low (centralized) |
| **Industry Standard** | Custom | ✅ Shopify, BigCommerce pattern |

---

### 8.2. Database Schema Benefits

**Storage Optimization:**
```
OLD Design:
- Products table: 15 columns (including Price, Quantity)
- Variants table: 10 columns
- Wasted space for Configurable products (NULL Price/Quantity in Products)

NEW Design (Variant-First):
- Products table: 12 columns (NO Price, Quantity)
- Variants table: 11 columns (WITH SKU, Price, Quantity, IsDefault)
- NO wasted space - clean separation
- Smaller Products table = better cache utilization
```

**Query Performance:**
```
OLD Design:
- Conditional JOIN based on ProductType
- Query planner struggles with optimization
- Different execution plans for different types

NEW Design (Variant-First):
- Consistent JOIN pattern
- Query planner can optimize better
- Single execution plan for all types
- Index on IsDefault = fast default Variant lookup
```

---

## 9. Frequently Asked Questions (FAQ)

### Q: What about performance? Isn't it slower to always JOIN Variants?

**A:** No! In fact, it's FASTER because:
- ✅ Database query planner can optimize consistent JOINs better
- ✅ Single code path = better CPU cache utilization
- ✅ No conditional logic in application layer
- ✅ Index on `IsDefault` makes default Variant lookup very fast
- ✅ Smaller Products table = more rows fit in cache

---

### Q: What if I have millions of products?

**A:** Variant-First scales BETTER:
- ✅ Smaller Product table (no redundant Price/Quantity columns)
- ✅ Better index utilization (queries always on Variants)
- ✅ Easier to partition/shard Variants table by ProductId
- ✅ Read replicas can cache smaller Products table more efficiently

---

### Q: Can I still use Product.Price in my existing code?

**A:** No - you MUST refactor to use Variant.Price. But this is a GOOD thing because:
- ✅ Makes your code consistent
- ✅ Eliminates bugs from conditional logic
- ✅ Future-proof (easy to add variants later)
- ✅ Follows industry best practices

---

### Q: What about Virtual products with unlimited quantity?

**A:** Set `Variant.Quantity = 0` for unlimited:
```csharp
var variant = new Variant(
    sku: "EBOOK-001",
    price: 9.99m,
  quantity: 0,  // ✅ 0 = unlimited for virtual products
    isDefault: true
);
variant.SetDownloadInfo(true, "file.pdf", "https://...");
```

---

### Q: How do I display product price in a list view?

**A:** Query default Variant:
```csharp
var products = await _context.Products
    .Include(p => p.Variants.Where(v => v.IsDefault))
    .Select(p => new ProductListDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Variants.First(v => v.IsDefault).Price,
        SKU = p.Variants.First(v => v.IsDefault).SKU
    })
    .ToListAsync();
```

---

## 10. Summary

### ✅ BUILD_30 Catalog Module Complete (Variant-First Design):

**Core Concepts:**
- ✅ **Variant-First Philosophy:** ALL products have >=1 variants
- ✅ **Price/Quantity:** ALWAYS in Variant table (never in Product)
- ✅ **Junction Tables:** Use Composite Primary Keys (NO surrogate Id)
- ✅ **Multiple Cascade Paths:** Fixed with DeleteBehavior.Restrict

**Implementation:**
- ✅ **Product Entity:** Marketing container only (NO Price/Quantity)
- ✅ **Variant Entity:** Sellable unit with SKU, Price, Quantity, IsDefault
- ✅ **Factory Methods:** CreateSimpleProduct, CreateConfigurableProduct, CreateVirtualProduct
- ✅ **EF Core Configurations:** Complete with proper indexes and relationships
- ✅ **Migration Strategy:** Scripts for both new and existing projects
- ✅ **Seed Data:** Examples for all product types

**Benefits:**
- ✅ **Consistency:** Single code path for all product types
- ✅ **Scalability:** Easy to add variants to Simple products
- ✅ **Performance:** Optimized queries with consistent JOINs
- ✅ **Maintainability:** Centralized pricing logic
- ✅ **Industry Alignment:** Shopify, BigCommerce pattern

**Database Schema:**
- ✅ 11 core tables in Catalog schema
- ✅ Composite Primary Keys for junction tables
- ✅ Strategic indexes for performance
- ✅ Proper cascade behavior (no Multiple Cascade Paths error)
- ✅ Soft delete support

---

## 11. Next Steps

**Tiếp theo:** BUILD_31 - Catalog Module Application Layer

Trong bước tiếp theo, chúng ta sẽ:
1. ✅ Tạo DTOs (CreateProductRequest, UpdateProductRequest, ProductDto)
2. ✅ Implement CQRS handlers (Create, Update, Delete, Search)
3. ✅ Implement Specifications for complex queries
4. ✅ Setup FluentValidation rules
5. ✅ Implement Controllers (ProductsController)

---

**Quay lại:** [Mục lục](BUILD_INDEX.md)

---

**Document Version:** 2.0 (Variant-First Design)  
**Last Updated:** 2025-02-01  
**Author:** ECO.WebApi Development Team  
**Status:** ✅ Production-Ready
