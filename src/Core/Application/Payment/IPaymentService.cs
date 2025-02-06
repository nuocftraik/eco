

using System.Transactions;
using ECO.WebApi.Application.Payment.Models;

namespace ECO.WebApi.Application.Payment;
public interface IPaymentService : ITransientService
{
    Task CreatePaymentAsync(Domain.Payment.Payment payment);
    Task<PaymentDto> GetPaymentByIdAsync(Guid paymentId);

    Task UpdatePaymentAsync(Guid paymentId);

    Task CreateTransactionAsync(Domain.Payment.Transaction transaction);


}
