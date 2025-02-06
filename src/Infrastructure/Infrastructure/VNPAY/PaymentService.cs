
using System.Threading.Tasks;
using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Application.Payment;
using ECO.WebApi.Application.Payment.Models;
using ECO.WebApi.Domain.Payment;
using ECO.WebApi.Infrastructure.Persistence.Context;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.VNPAY;
public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreatePaymentAsync(Payment payment)
    {   
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

   

    public async Task<PaymentDto> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment =  await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        return payment.Adapt<PaymentDto>();
    }

    public async Task UpdatePaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
        payment.LastModifiedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

    }

    public async Task CreateTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
}
