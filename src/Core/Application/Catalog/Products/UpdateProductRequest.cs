
using ECO.WebApi.Domain.Attributes;
using ECO.WebApi.Domain.Enum;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Products;
public class UpdateProductRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public ProductType ProductType { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public ProductStatus Status { get; set; }
    public string? Description { get; set; }
    public string? MainImage { get; set; }
    public List<AttributeDto>? Attributes { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}

//Validator
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        // ProductType validation
        RuleFor(x => x.ProductType)
            .IsInEnum().WithMessage("ProductType is not valid.");

        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");

        // Slug validation
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(255).WithMessage("Slug cannot exceed 255 characters.");

        // CategoryIds validation
        RuleFor(x => x.CategoryIds)
          .Must(x => x == null || x.All(id => id != Guid.Empty)).WithMessage("CategoryIds cannot contain empty GUID.");
    }
}

//Handler
public class UpdateProductRequestHandler : IRequestHandler<UpdateProductRequest, Guid>
{
    private readonly IRepository<Product> _productRepository;

    public UpdateProductRequestHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id));
        product.Update(request.ProductType, request.Name, request.Slug ,request.Status, request.Description, request.MainImage);

        product.UpdateCategories(request.CategoryIds);

        var newAttributes = request.Attributes?.Adapt<List<Domain.Attributes.Attribute>>();
        product.UpdateAttributes(newAttributes);
        await _productRepository.UpdateAsync(product);
        return product.Id;
    }
}
