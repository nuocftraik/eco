
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECO.WebApi.Domain.Enum;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Reflection.Emit;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
           .ToTable("Products", SchemaNames.Catalog);
        builder.Property(x => x.Name)
           .HasMaxLength(225)
           .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(225)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Description)
         .HasMaxLength(1024);

        builder.Property(x => x.ViewCount).HasDefaultValue(0);
        builder.Property(x => x.Status).HasDefaultValue(ProductStatus.InStock);

        builder
            .Property(p => p.MainImage)
                .HasMaxLength(2048);
    }
}
public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder
            .ToTable("ProductCategories", SchemaNames.Catalog);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder
          .ToTable("Categories", SchemaNames.Catalog);
        builder.Property(x => x.Name)
                .HasMaxLength(225)
                .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(225)
            .IsUnicode(false)
            .IsRequired();

    }
}


public class UserReviewConfiguration : IEntityTypeConfiguration<UserReview>
{
    public void Configure(EntityTypeBuilder<UserReview> builder)
    {
        builder
          .ToTable("UserReviews", SchemaNames.Catalog);
        builder.Property(x => x.Content)
                       .HasMaxLength(1025)
                       .IsRequired();
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder
            .ToTable("Tags", SchemaNames.Catalog);
        builder.Property(x => x.Id)
         .HasMaxLength(50)
         .IsRequired();
        builder.Property(x => x.Name)
           .HasMaxLength(50)
           .IsRequired();
        builder.Property(x => x.Slug)
          .HasMaxLength(50)
          .IsRequired();

    }
}

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder
            .ToTable("ProductTags", SchemaNames.Catalog);
    }
}

public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder
            .ToTable("VariantAttributeValues", SchemaNames.Catalog);

        builder
            .HasOne(vav => vav.Variant)
            .WithMany(v => v.VariantAttributeValues)
            .HasForeignKey(vav => vav.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(vav => vav.AttributeValue)
            .WithMany(av => av.VariantAttributeValues)
            .HasForeignKey(vav => vav.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder
          .ToTable("Variants", SchemaNames.Catalog);
        builder.Property(x => x.SKU)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.MainImage)
          .HasMaxLength(1024);
        builder.Property(x => x.Status).HasDefaultValue(ProductStatus.InStock);

    }
}
