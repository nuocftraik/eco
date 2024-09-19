

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;
public class Product : AuditableEntity, IAggregateRoot
{
    //Basic Info
    public ProductType ProductType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    //Media
    public string? MainImage { get; private set; }
    public ProductStatus Status { get; private set; }
    public int ViewCount { get; private set; }

    //Navigation
    public List<Variant> Variants { get; private set; } = new();
    public List<Attributes.Attribute> Attributes { get; private set; } = new();
    public List<ProductCategory> ProductCategories { get; private set; } = new();
    public List<ProductTag> ProductTags { get; private set; } = new();


    //Methods
    private Product() { } 

    public Product(ProductType productType, string name, string slug, string? description, string? mainImage)
    {
        ProductType = productType;
        Name = name;
        Slug = slug;
        Description = description;
        MainImage = mainImage;
        Status = ProductStatus.InStock;
        ViewCount = 0;
    }

    // Method to update product
    public void Update(ProductType productType, string name, string slug, ProductStatus status, string? description, string? mainImage)
    {
        ProductType = productType;
        Name = name;
        Slug = slug;
        Status = status;
        Description = description;
        MainImage = mainImage;
    }


    public void AddCategory(Guid categoryId)
    {
        ValidateId(categoryId);
        if (HasCategory(categoryId))
        {
            throw new InvalidOperationException($"Category with ID {categoryId} is already added.");
        }
        ProductCategories.Add(new ProductCategory(Id, categoryId));
    }

    public void UpdateCategories(List<Guid>? newCategoryIds)
    {
        if (newCategoryIds == null || newCategoryIds.Count == 0)
        {
            ProductCategories.Clear();
            return;
        }

        ProductCategories.RemoveAll(pc => !newCategoryIds.Contains(pc.ProductId));

        // Tạo HashSet từ danh sách category hiện tại để tối ưu hóa việc kiểm tra tồn tại
        var existingCategoryIds = ProductCategories.Select(pc => pc.CategoryId).ToHashSet(); 
        foreach (var categoryId in newCategoryIds)
        {
            if (!existingCategoryIds.Contains(categoryId))
            {
                ProductCategories.Add(new ProductCategory(Id, categoryId));
            }
        }
    }

    public Product ClearMainImagePath()
    {
        MainImage = string.Empty;
        return this;
    }
    private void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }
    }

    private bool HasCategory(Guid categoryId)
    {
        return ProductCategories.Any(pc => pc.CategoryId == categoryId);
    }

    // Method to add a variant
    public void AddVariant(Variant variant)
    {
        Variants.Add(variant);
    }

    // Method to remove a variant
    public void RemoveVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new ArgumentException($"Variant with ID {variantId} was not found.");
        Variants.Remove(variant);
    }

    // Add Attribute
    public void AddAttribute(string name, AttributeType attributeType, List<string> attributeValues)
    {
        var attribute = new Domain.Attributes.Attribute(name, attributeType, Id);
        foreach (var value in attributeValues)
        {
            attribute.AddValue(value);
        }
        Attributes.Add(attribute);
    }

    public void UpdateAttributes(List<Domain.Attributes.Attribute>? newAttributes)
    {
        if (newAttributes == null || newAttributes.Count == 0)
        {
            Attributes.Clear();
            return;
        }

        Attributes.RemoveAll(a => !newAttributes.Any(na => na.Id == a.Id));

        // Cập nhật các thuộc tính hoặc thêm mới các thuộc tính
        foreach (var newAttribute in newAttributes)
        {
            var existingAttribute = Attributes.FirstOrDefault(a => a.Id == newAttribute.Id);
            if (existingAttribute != null)
            {
                // Nếu thuộc tính đã tồn tại, cập nhật các giá trị
                existingAttribute.UpdateAttributeValues(newAttribute.AttributeValues);
            }
            else
            {
                Attributes.Add(newAttribute);
            }
        }
    }



}
