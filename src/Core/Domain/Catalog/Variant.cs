using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;
public class Variant : BaseEntity
{
    //Basic Info
    public Guid ProductId { get; set; }
    public ProductStatus Status { get; set; }
    //Media
    public string? MainImage { get; set; }
    //Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    //Identifiers
    public string SKU { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }


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
    public virtual Product Product { get; set; }
    public virtual List<UserReview> UserReviews { get; set; } = new();
    public virtual List<VariantAttributeValue> VariantAttributeValues { get; set; } = new();

    //Methods
    private Variant() { }

    // Constructor for creating new variant
    public Variant(Guid productId, string sku, double price, string? image, bool isActive, bool isDefault)
    {
        ProductId = productId;
        SKU = sku;
        Price = price;
        MainImage = image;
        IsActive = isActive;
        IsDefault = isDefault;
        Status = ProductStatus.InStock;
        Quantity = 0;
    }
    public void Update(string sku, double price, string? mainImage, bool isActive, bool isDefault,
                   ProductStatus status, bool trackInventory, int? quantity,
                   bool requireShipping, double? weight, double? width, double? height, double? length,
                   bool includeDownload, string? fileName, string? fileUrl, double? comparePrice = 0)
    {
        // Cập nhật thông tin cơ bản
        SKU = sku;
        Price = price;
        MainImage = mainImage;
        IsActive = isActive;
        IsDefault = isDefault;
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

    public void AddAttributeValue(List<Guid> attributeValueIds)
    {
        foreach (var valueId in attributeValueIds)
        {
            VariantAttributeValues.Add(new VariantAttributeValue(valueId));
        }
    }

    public void UpdateAttributeValues(List<Guid> newAttributeValueIds)
    {
        // Xóa những giá trị không còn trong danh sách mới
        VariantAttributeValues.RemoveAll(vav => !newAttributeValueIds.Contains(vav.AttributeValueId));

        // Tạo HashSet để tối ưu hóa kiểm tra tồn tại
        var existingAttributeValueIds = VariantAttributeValues.Select(vav => vav.AttributeValueId).ToHashSet();

        // Thêm các giá trị mới nếu chưa tồn tại
        foreach (var attributeValueId in newAttributeValueIds)
        {
            if (!existingAttributeValueIds.Contains(attributeValueId))
            {
                VariantAttributeValues.Add(new VariantAttributeValue(attributeValueId));
            }
        }
    }



}
