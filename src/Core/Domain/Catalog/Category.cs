using System.ComponentModel.DataAnnotations.Schema;


namespace ECO.WebApi.Domain.Catalog;
public class Category : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Image { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentId { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual Category? CategoryParent { get; set; }
    public virtual List<Category>? CategoryChildrens { get; set; }
    public virtual List<ProductCategory> ProductCategories { get; set; }

}
