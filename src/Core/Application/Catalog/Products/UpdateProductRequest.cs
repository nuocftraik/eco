
using ECO.WebApi.Domain.Attributes;
using ECO.WebApi.Domain.Enum;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Products;
public class UpdateProductRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string? Description { get; set; }
    public string? MainImage { get; set; }
    public List<AttributeDto>? Attributes { get; set; }
    public List<Guid>? CategoryIds { get; set; }

    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }


    // Inventory
    public int? Quantity { get; set; }

    // Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
}

//Validator
public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {

        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");

        // Slug validation
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(255).WithMessage("Slug cannot exceed 255 characters.");
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
        if (product.ProductType == ProductType.Simple)
        {
            product.UpdateSimpleProduct(request.Price,request.Quantity.Value, request.IncludeDownload, request.FileName, request.FileUrl,request.MainImage,request.ComparePrice);
        }
        else
        {
            product.UpdateConfigurableProduct(request.Name, request.Slug,request.Description, request.MainImage);
            var newAttributes = request.Attributes?.Adapt<List<Domain.Attributes.Attribute>>();
            product.UpdateAttributes(newAttributes);
        }

        product.UpdateCategories(request.CategoryIds);

      
        await _productRepository.UpdateAsync(product);
        return product.Id;
    }
}
