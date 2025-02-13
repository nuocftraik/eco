

using ECO.WebApi.Application.Ordering.Orders.Events;
using ECO.WebApi.Application.Payment.Models;
using ECO.WebApi.Domain.Payment;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ECO.WebApi.Application.Payment;
public class ProcessPaymentResultRequest : IRequest<PaymentResult>
{
    public IQueryCollection Query { get; }

    public ProcessPaymentResultRequest(IQueryCollection query)
    {
        Query = query;
    }
}

public class ProcessPaymentResultRequestHandler : IRequestHandler<ProcessPaymentResultRequest, PaymentResult>
{
    private readonly IVnpay _vnpay;
    private readonly IRepository<Domain.Payment.Payment> _paymentRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IEventPublisher _eventPublisher;
    public ProcessPaymentResultRequestHandler(IVnpay vnpay, IRepository<Domain.Payment.Payment> paymentRepository, IRepository<Transaction> transactionRepository, IEventPublisher eventPublisher)
    {
        _vnpay = vnpay;
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<PaymentResult> Handle(ProcessPaymentResultRequest request, CancellationToken cancellationToken)
    {
        var paymentResult = _vnpay.GetPaymentResult(request.Query);
        var payment = await _paymentRepository.GetByIdAsync(paymentResult.PaymentId) ?? throw new NotFoundException("Không tìm thấy giao dịch");

        if (paymentResult.IsSuccess)
        {

            payment.LastModifiedOn = DateTime.UtcNow;  // cập nhật thời gian thanh toán
            await _paymentRepository.UpdateAsync(payment);

            var transaction = new Transaction(payment.Id, paymentResult.VnpayTransactionId, paymentResult.PaymentMethod,
                                              paymentResult.PaymentResponse.Code, paymentResult.TransactionStatus.Code,
                                              paymentResult.BankingInfor.BankCode, paymentResult.BankingInfor.BankTransactionId);

            await _transactionRepository.AddAsync(transaction);

            // 🔔 Phát sự kiện OrderPaidSuccessEvent
            await _eventPublisher.PublishAsync(new OrderPaidSuccessEvent(payment.OrderId, payment.Id));

            return paymentResult;
        }
        else
        {
            // 🔔 Phát sự kiện OrderPaidFailedEvent
            await _eventPublisher.PublishAsync(new OrderPaidFailedEvent(payment.OrderId, payment.Id));

            return paymentResult;
        }
    }
}
