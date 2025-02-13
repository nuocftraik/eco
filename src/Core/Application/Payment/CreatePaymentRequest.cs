

using ECO.WebApi.Application.Ordering.Orders.Events;
using ECO.WebApi.Application.Payment.Helpers;
using ECO.WebApi.Application.Payment.Models;
using ECO.WebApi.Domain.Payment.Enums;
using Microsoft.AspNetCore.Http;
namespace ECO.WebApi.Application.Payment;
public class CreatePaymentRequest : IRequest<string>
{
    public Guid OrderId { get; }
    public double Money { get; }
    public string Description { get; }
    public Guid? AnonymousUserId { get; }

    public CreatePaymentRequest(Guid orderId, double money, string description, Guid? anonymousUserId)
    {
        OrderId = orderId;
        Money = money;
        Description = description;
        AnonymousUserId = anonymousUserId;
    }
}

public class CreatePaymentRequestHandler : IRequestHandler<CreatePaymentRequest, string>
{
    private readonly IVnpay _vnpay;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<Domain.Payment.Payment> _paymentRepository;

    public CreatePaymentRequestHandler(IVnpay vnpay, IRepository<Domain.Payment.Payment> paymentRepository, IHttpContextAccessor httpContextAccessor)
    {
        _vnpay = vnpay;
        _paymentRepository = paymentRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> Handle(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = NetworkHelper.GetIpAddress(_httpContextAccessor.HttpContext);
        var payment = new Domain.Payment.Payment(request.OrderId, request.Money, PaymentProvider.VNPAY, request.Description,
                                  DisplayLanguage.Vietnamese, BankCode.ANY, Currency.VND, ipAddress);

        var paymentCreated = await _paymentRepository.AddAsync(payment);

        var paymentRequest = new PaymentRequest
        {
            PaymentId = payment.Id,
            Money = request.Money,
            Description = request.Description,
            IpAddress = ipAddress,
            BankCode = BankCode.ANY,
            Currency = Currency.VND,
            Language = DisplayLanguage.Vietnamese,
            CreatedDate = payment.CreatedOn,
        };

        return _vnpay.GetPaymentUrl(paymentRequest);
    }
}
