

namespace ECO.WebApi.Application.Catalog.Products;
public class DeleteProductRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteProductRequest(Guid id)
    {
        Id = id;
    }
}

public class DeleteProductRequestValidator : AbstractValidator<DeleteProductRequest>
{
    public DeleteProductRequestValidator()
    {
        
    }
}

//Handler
public class DeleteProductRequestHandler : IRequestHandler<DeleteProductRequest, Guid>
{
    private readonly IRepository<Product> _repository;

    public DeleteProductRequestHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id))
              ?? throw new NotFoundException($"Product with ID {request.Id} was not found.");

        await _repository.DeleteAsync(product);
        return product.Id;
    }
}

