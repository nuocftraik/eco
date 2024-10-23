

using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Application.Catalog.Products;
public class DeleteVariantRequest : IRequest<Guid>
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
    private readonly IRepository<Variant> _variantRepository;
    public DeleteVariantRequestHandler(IRepository<Product> productRepository, IRepository<Variant> variantRepository)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
    }

    public async Task<Guid> Handle(DeleteVariantRequest request, CancellationToken cancellationToken)
    {
        // Lấy Product từ DB
        var product = await _productRepository.FirstOrDefaultAsync(new ProductByIdSpec(request.ProductId), cancellationToken)
          ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        product.Attributes
       .SelectMany(attr => attr.AttributeValues)
       .ToList()
       .ForEach(av => av.VariantAttributeValues.Clear());

        await _productRepository.UpdateAsync(product,cancellationToken);

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.Id)
        ?? throw new NotFoundException($"Variant with ID {request.Id} was not found.");
        await _variantRepository.DeleteAsync(variant);

        return request.Id;
    }
}
