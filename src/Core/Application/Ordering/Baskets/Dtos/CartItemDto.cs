

using ECO.WebApi.Application.Catalog.Products;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Ordering.Baskets;
public class CartItemDto
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public ProductStatus Status { get; set; }
    // Media
    public string? MainImage { get; set; }
    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    public bool IsDefault { get; set; }


    public int Quantity { get; set; }

    // Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string ProductName { get; set; }
    public string ProductImage { get; set; }
    public List<AttributeValueDto> AttributeValues { get; set; }
}
