

using System.Diagnostics;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;
public class Product : AuditableEntity, IAggregateRoot
{
    //Basic Info
    public ProductType ProductType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public ProductStatus Status { get; private set; }
    //Media
    public string? MainImage { get; private set; }
    //Billing
    public double? Price { get; set; }
    public double? ComparePrice { get; set; }

    //Identifiers
    public string? SKU { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; private set; }
    //Inventory
    public bool TrackInventory { get; set; }
    public int? Quantity { get; set; }
    //Shipping
    public bool RequireShipping { get; set; }
    public double? Weight { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Length { get; set; }

    //Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }

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
    public void UpdateConfigurableProduct(string name, string slug, ProductStatus status, string? description, string? mainImage)
    {
        Name = name;
        Slug = slug;
        Status = status;
        Description = description;
        MainImage = mainImage;
    }

    public void UpdateSimpleProduct(string sku, double price, string? mainImage, bool isActive,
                   ProductStatus status, bool trackInventory, int? quantity,
                   bool requireShipping, double? weight, double? width, double? height, double? length,
                   bool includeDownload, string? fileName, string? fileUrl, double? comparePrice = 0)
    {
        // Cập nhật thông tin cơ bản
        SKU = sku;
        Price = price;
        MainImage = mainImage;
        IsActive = isActive;
        Status = status;

        // Cập nhật thông tin billing
        TrackingBilling(price, comparePrice);

        // Cập nhật thông tin kho hàng
        TrackingInventory(trackInventory, quantity);

        // Cập nhật thông tin shipping
        TrackingShipping(requireShipping, weight, width, height, length);

        // Cập nhật thông tin download nếu có
        TrackingDownload(includeDownload, fileName, fileUrl);


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


    //Billing
    public void TrackingBilling(double price, double? comparePrice = 0)
    {
        Price = price;
        if (comparePrice <= Price)
        {
            throw new ArgumentException("Compare price must be greater than the actual price.");
        }
        ComparePrice = comparePrice;
    }

    //Tracking inventory
    public void TrackingInventory(bool trackInventory, int? quantity = 0)
    {
        TrackInventory = trackInventory;
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater or equal 0.");
        }
        Quantity = quantity;
    }

    public void TrackingDownload(bool includeDownload, string? fileName, string? fileUrl)
    {
        IncludeDownload = includeDownload;
        if (includeDownload)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("FileName cannot be null or empty.", nameof(fileName));

            if (!Uri.IsWellFormedUriString(fileUrl, UriKind.Absolute))
                throw new ArgumentException("FileUrl must be a valid URL.", nameof(fileUrl));
        }
        FileName = fileName;
        FileUrl = fileUrl;
    }


    public void TrackingShipping(bool requireShipping, double? weight = 0, double? width = 0, double? height = 0, double? length = 0)
    {
        RequireShipping = requireShipping;
        if (requireShipping)
        {
            if (weight <= 0 || width <= 0 || height <= 0 || length <= 0)
            {
                throw new ArgumentException("Dimensions must be greater than or equal to zero.");
            }
        }
        Weight = weight;
        Width = width;
        Height = height;
        Length = length;
    }

}
