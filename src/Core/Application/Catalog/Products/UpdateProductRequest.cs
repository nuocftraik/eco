
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

    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    // Identifiers
    public string SKU { get; set; }
    public bool IsActive { get; set; }

    // Inventory
    public bool TrackInventory { get; set; }
    public int? Quantity { get; set; }

    // Shipping
    public bool RequireShipping { get; set; }
    public double? Weight { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Length { get; set; }

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
        if (product.ProductType == ProductType.Simple)
        {
            product.UpdateSimpleProduct(request.SKU, request.Price, request.MainImage, request.IsActive,product.Status, request.TrackInventory, request.Quantity, request.RequireShipping, request.Weight, request.Width, request.Height, request.Length, request.IncludeDownload, request.FileName, request.FileUrl);
        }
        else
        {
            product.UpdateConfigurableProduct(request.ProductType, request.Name, request.Slug, request.Status, request.Description, request.MainImage);
        }

        product.UpdateCategories(request.CategoryIds);

        var newAttributes = request.Attributes?.Adapt<List<Domain.Attributes.Attribute>>();
        product.UpdateAttributes(newAttributes);
        await _productRepository.UpdateAsync(product);
        return product.Id;
    }
}
