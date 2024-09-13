
using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Domain.Ordering;


namespace ECO.WebApi.Domain.Catalog;

public class UserReview : AuditableEntity, IAggregateRoot
{
    public double? Rating { get; set; }
    public string Content { get; set; }
    public Guid UserId { get; set; }
    public Guid VariantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ParentId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; }
    [ForeignKey(nameof(VariantId))]
    public virtual Variant Variant { get; set; }
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; }
    [ForeignKey(nameof(ParentId))]
    public virtual UserReview? UserReviewParent { get; set; }
    public virtual ICollection<UserReview>? UserReviewChildrens { get; set; }

}
