

namespace ECO.WebApi.Domain.Payment;
public class Payment : AuditableEntity ,IAggregateRoot
{
    public string Content { get; set; }
    public string Currency { get; set; }
    public string RefId { get; set; }
    public decimal? RequiredAmount { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string? Language { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? PaymentDestinationId { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public string? LastMessage { get; set; }
}
