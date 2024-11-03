

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;
public class ProductInListDto
{
    //Basic Info
    public Guid Id { get; set; }
    public ProductType ProductType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    //Media
    public string? MainImage { get; private set; }
    public ProductStatus Status { get; private set; }
    public int ViewCount { get; private set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

}
