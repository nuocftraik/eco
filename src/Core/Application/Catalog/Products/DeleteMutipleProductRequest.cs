
namespace ECO.WebApi.Application.Catalog.Products;
internal class DeleteMutipleProductRequest : IRequest<List<Guid>>
{
    public List<Guid> Ids { get; set; }
}

//Handler
internal class DeleteMutipleProductHandler : IRequestHandler<DeleteMutipleProductRequest, List<Guid>>
{
    private readonly IRepository<Product> _repository;
    private readonly IMediator _mediator;

    public DeleteMutipleProductHandler(IRepository<Product> repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<List<Guid>> Handle(DeleteMutipleProductRequest request, CancellationToken cancellationToken)
    {
        var productIds = new List<Guid>();
        foreach (var id in request.Ids)
        {
            await _mediator.Send(new DeleteProductRequest(id));
            productIds.Add(id);
        }
        return productIds;
    }
}
