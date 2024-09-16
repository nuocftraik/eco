namespace ECO.WebApi.Application.Catalog.Categories;
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public string Creator { get; set; }
    public List<ProductInCategoryDto> Products { get; set; }

}
