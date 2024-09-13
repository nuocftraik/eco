
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Domain.Ordering;


namespace ECO.WebApi.Domain.Catalog;

public class UserReview : AuditableEntity, IAggregateRoot
{
    public double? Rating { get; set; }
    public string Content { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid? ParentId { get; set; }

    [ForeignKey(nameof(OrderItemId))]
    public virtual OrderItem OrderItem { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual UserReview? UserReviewParent { get; set; }
    public virtual List<UserReview>? UserReviewChildrens { get; set; } = new();

}

