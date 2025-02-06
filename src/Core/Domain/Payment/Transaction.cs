
using ECO.WebApi.Domain.Payment.Enums;

namespace ECO.WebApi.Domain.Payment;
public class Transaction : AuditableEntity, IAggregateRoot
{
    public Guid PaymentId { get; set; }
    public long VnpayTransactionId { get; set; }
    public string PaymentMethod { get; set; }
    public ResponseCode ResponseCode { get; set; }
    public TransactionStatusCode? StatusCode { get; set; } 
    public string BankCode { get; set; }
    public string BankTransactionId { get; set; }
    public virtual Payment Payment { get; set; }

    public Transaction(Guid paymentId, long vnpayTransactionId, string paymentMethod, ResponseCode responseCode, TransactionStatusCode? statusCode, string bankCode, string bankTransactionId)
    {
        PaymentId = paymentId;
        VnpayTransactionId = vnpayTransactionId;
        PaymentMethod = paymentMethod;
        ResponseCode = responseCode;
        StatusCode = statusCode;
        BankCode = bankCode;
        BankTransactionId = bankTransactionId;
    }
}
