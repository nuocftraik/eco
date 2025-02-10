using ECO.WebApi.Domain.Common.Exceptions;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Domain.Catalog;
public class Variant : BaseEntity, IAggregateRoot
{
    //Basic Info
    public Guid ProductId { get; private set; }
    public ProductStatus Status { get; private set; }
    //Media
    public string? MainImage { get; private set; }
    //Billing
    public double Price { get; private set; }
    public double? ComparePrice { get; private set; }

    //Identifiers
    public bool IsDefault { get; private set; }
    public int? Quantity { get; private set; }
    //Downloadable
    public bool IncludeDownload { get; private set; }
    public string? FileName { get; private set; }
    public string? FileUrl { get; private set; }
    //Navigation
    public virtual Product Product { get; private set; }
    public virtual List<UserReview> UserReviews { get; private set; } = new();
    public virtual List<VariantAttributeValue> VariantAttributeValues { get; private set; } = new();
    public Variant()
    {
        
    }

    // Constructor for creating new variant
    public Variant(bool isDefault,int quantity, string? image, double price,double? comparePrice, bool includeDownload,string? fileName ,string fileUrl)
    {
      
        MainImage = image;
        IsDefault = isDefault;
        Quantity = quantity;
        Status = ProductStatus.InStock;
        TrackingBilling(price, comparePrice);
        TrackingDownload(includeDownload, fileName, fileUrl);
    }   
    public void Update(bool isDefault, double price, string? mainImage ,double? comparePrice,bool includeDownload, int? quantity, string? fileName, string? fileUrl)
    {
        // Cập nhật thông tin cơ bản
        Price = price;
        MainImage = mainImage;
        IsDefault = isDefault;
        Quantity = quantity;
        // Cập nhật thông tin billing
        TrackingBilling(price, comparePrice);
        // Cập nhật thông tin download nếu có
        TrackingDownload(includeDownload, fileName, fileUrl);
    }


    //Billing
    public void TrackingBilling(double price, double? comparePrice = 0)
    {
        Price = price;
        if (comparePrice <= Price)
        {
            throw new BadRequestException("Compare price must be greater than the actual price.");
        }
        ComparePrice = comparePrice;
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


    public void AddAttributeValue(List<Guid> attributeValueIds)
    {
        foreach (var valueId in attributeValueIds)
        {
            VariantAttributeValues.Add(new VariantAttributeValue(valueId));
        }
    }

    public void UpdateAttributeValues(List<Guid> newAttributeValueIds)
    {
        // Xóa tất cả các VariantAttributeValues hiện có trước
        VariantAttributeValues.Clear();

        // Thêm các giá trị mới
        foreach (var attributeValueId in newAttributeValueIds)
        {
            VariantAttributeValues.Add(new VariantAttributeValue(attributeValueId));
        }
    }


}
