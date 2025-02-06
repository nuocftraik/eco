

using ECO.WebApi.Domain.Payment.Enums;

namespace ECO.WebApi.Application.Payment.Models;
public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public double Amount { get; set; }
    public PaymentProvider Provider { get; set; } // Enum: VNPAY, MOMO, ZALOPAY
    public string Description { get; set; }
    public DisplayLanguage Language { get; set; }
    public BankCode BankCode { get; set; }
    public Currency Currency { get; set; }
}
