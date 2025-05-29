
using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Common.Events;
using ECO.WebApi.Domain.Enum;
using ECO.WebApi.Domain.Ordering;

namespace ECO.WebApi.Application.Ordering.Orders;
public class CreateOrderRequest : IRequest<Guid> ,IHasAnonymousId
{
    public Guid? AnonymousId { get; set; }
    public PaymentMethod PaymentMethod { get;  set; }
    public double Total { get;  set; }
    public string CustomerName { get; set; }
    public string CustomerPhoneNumber { get;  set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();

}

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
                 .IsInEnum().WithMessage("PaymentMethod is not valid.");

        RuleFor(x => x.Total)
            .GreaterThan(0).WithMessage("Total must be greater than 0.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required.")
            .MaximumLength(255).WithMessage("CustomerName cannot exceed 255 characters.");

        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty().WithMessage("CustomerPhoneNumber is required.")
            .MaximumLength(255).WithMessage("CustomerPhoneNumber cannot exceed 255 characters.");


        RuleFor(x => x.OrderItems)
            .NotEmpty().WithMessage("OrderItems is required.")
            .Must(x => x.All(item => item.Quantity > 0)).WithMessage("Quantity must be greater than 0.");
    }
}

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, Guid>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IMediator _mediator;
    public CreateOrderRequestHandler(IRepository<Order> orderRepository, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new Order(request.PaymentMethod,request.Total,request.CustomerName,request.CustomerPhoneNumber,request.CustomerEmail, request.CustomerAddress);

        foreach (var item in request.OrderItems)
        {
            order.AddOrderItem(item.VariantId, item.Price, item.Quantity);
        }
        var orderCreated = await _orderRepository.AddAsync(order);



        // Add Domain Events to be raised after the commit
        orderCreated.DomainEvents.Add(EntityCreatedEvent.WithEntity(orderCreated));


        return order.Id;
    }
}
