using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Enum;
using ECO.WebApi.Domain.Identity;


namespace ECO.WebApi.Domain.Ordering;
public class OrderTransaction : AuditableEntity, IAggregateRoot
{
    public Guid OrderId { get; set; }
    public TransactionType TransactionType { get; set; }
    public string? AccountNumber { get; set; }
    public string? Note { get; set; }
    public double Amount { get; set; } // Số tiền của giao dịch
    public string Currency { get; set; } // Đơn vị tiền tệ của giao dịch (ví dụ: USD, EUR, v.v.)
    public DateTime ExpireDate { get; set; } 
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; }
}
