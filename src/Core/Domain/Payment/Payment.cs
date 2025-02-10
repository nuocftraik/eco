

using System.ComponentModel.DataAnnotations.Schema;
using ECO.WebApi.Domain.Ordering;
using ECO.WebApi.Domain.Payment.Enums;

namespace ECO.WebApi.Domain.Payment;
public class Payment : AuditableEntity ,IAggregateRoot
{
    public Guid OrderId { get; set; }
    public double Amount { get; set; }
    public PaymentProvider Provider { get; set; } // Enum: VNPAY, MOMO, ZALOPAY
    public string Description { get; set; }
    public DisplayLanguage Language { get; set; }
    public BankCode BankCode { get; set; }
    public Currency Currency { get; set; }
    public string IpAddress { get; set; }
    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; }

    public Payment(Guid orderId, double amount, PaymentProvider provider, string description, DisplayLanguage language, BankCode bankCode, Currency currency, string ipAddress)
    {
        OrderId = orderId;
        Amount = amount;
        Provider = provider;
        Description = description;
        Language = language;
        BankCode = bankCode;
        Currency = currency;
        IpAddress = ipAddress;
    }
}



