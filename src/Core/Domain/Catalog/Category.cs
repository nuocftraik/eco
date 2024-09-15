using System.ComponentModel.DataAnnotations.Schema;


namespace ECO.WebApi.Domain.Catalog;
public class Category : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public virtual List<ProductCategory> ProductCategories { get; set; }

}
