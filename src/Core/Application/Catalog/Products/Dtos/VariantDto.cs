

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;
public class VariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; } 
    public ProductStatus Status { get; set; }

    // Media
    public string? MainImage { get; set; }

    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    // Identifiers
    public string SKU { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }

    // Inventory
    public bool TrackInventory { get; set; }
    public int? Quantity { get; set; }

    // Shipping
    public bool RequireShipping { get; set; }
    public double? Weight { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Length { get; set; }

    // Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public List<AttributeValueDto> AttributeValues { get; set; }
}
