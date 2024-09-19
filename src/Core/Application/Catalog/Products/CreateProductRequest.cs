
using ECO.WebApi.Application.Catalog.Products.Dtos;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;
public class CreateProductRequest : IRequest<Guid>
{
    public ProductType ProductType { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public string? MainImage { get; set; }
    public List<CreateAttributeDto>? Attributes { get; set; }
    public List<Guid>? CategoryIds { get; set; } 

}

//Validator
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
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
public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IRepository<Product> _productRepository;

    public CreateProductRequestHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.ProductType,request.Name,request.Slug,request.Description, request.MainImage);
        if (request.CategoryIds != null && request.CategoryIds.Count > 0)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                product.AddCategory(categoryId);
            }
        }
        if (request.Attributes != null && request.Attributes.Count > 0)
        {
            foreach (var attribute in request.Attributes)
            {
                product.AddAttribute(attribute.Name,attribute.AttributeType,attribute.Values);
            }
        }
        await _productRepository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}


