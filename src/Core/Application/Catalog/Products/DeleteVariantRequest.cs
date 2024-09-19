

namespace ECO.WebApi.Application.Catalog.Products;
internal class DeleteVariantRequest : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public Guid Id { get; set; }

}

internal class DeleteVariantRequestValidator : AbstractValidator<DeleteVariantRequest>
{
    public DeleteVariantRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Variant ID is required.")
            .Must(x => x != Guid.Empty).WithMessage("Variant ID cannot be empty GUID.");
    }
}

//Handler
internal class DeleteVariantRequestHandler : IRequestHandler<DeleteVariantRequest, Guid>
{
    private readonly IRepository<Product> _productRepository;

    public DeleteVariantRequestHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(DeleteVariantRequest request, CancellationToken cancellationToken)
    {
        // Lấy Product từ DB
        var product = await _productRepository.GetByIdAsync(new ProductByIdSpec(request.ProductId), cancellationToken)
          ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        product.RemoveVariant(request.Id);

        await _productRepository.UpdateAsync(product, cancellationToken);
        return request.Id;
    }
}
