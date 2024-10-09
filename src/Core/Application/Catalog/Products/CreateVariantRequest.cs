

using ECO.WebApi.Application.Catalog.Products.Factory;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;
public class CreateVariantRequest : IRequest<Guid>
{
    //Basic Info
    public Guid ProductId { get; set; }
    public ProductStatus Status { get; set; }
    //Media
    public string? MainImage { get; set; }
    //Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    //Identifiers
    public string SKU { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }


    //Inventory
    public bool TrackInventory { get; set; }
    public int? Quantity { get; set; }
    //Shipping
    public bool RequireShipping { get; set; }
    public double? Weight { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? Length { get; set; }

    //Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }

    public List<Guid> AttributeValueIds { get; set; }
}


//Validator
public class CreateVariantRequestValidator : AbstractValidator<CreateVariantRequest>
{
    public CreateVariantRequestValidator()
    {
        // ProductId validation
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.")
            .Must(x => x != Guid.Empty).WithMessage("ProductId cannot be empty GUID.");

        // Price validation
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        // ComparePrice validation: ComparePrice should be greater than Price if provided
        RuleFor(x => x.ComparePrice)
            .GreaterThan(x => x.Price).When(x => x.ComparePrice.HasValue)
            .WithMessage("ComparePrice must be greater than Price.");

        // SKU validation
        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.");

        // Quantity validation (if TrackInventory is true)
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).When(x => x.TrackInventory)
            .WithMessage("Quantity must be greater than or equal to 0 when inventory tracking is enabled.");

        // Shipping validation: If RequireShipping is true, check weight and dimensions
        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).When(x => x.RequireShipping)
            .WithMessage("Weight must be greater than 0 or equal if shipping is required.");

        RuleFor(x => x.Width)
            .GreaterThanOrEqualTo(0).When(x => x.RequireShipping)
            .WithMessage("Width must be greater than 0 or equal if shipping is required.");

        RuleFor(x => x.Height)
            .GreaterThanOrEqualTo(0).When(x => x.RequireShipping)
            .WithMessage("Height must be greater than or equal 0 if shipping is required.");

        RuleFor(x => x.Length)
            .GreaterThanOrEqualTo(0).When(x => x.RequireShipping)
            .WithMessage("Length must be greater than or equal 0 if shipping is required.");

        // Downloadable product validation: if IncludeDownload is true, validate FileName and FileUrl
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required if IncludeDownload is true.")
            .MaximumLength(100).WithMessage("FileName cannot exceed 100 characters.")
            .When(x => x.IncludeDownload);
    }
}

//Handler
public class CreateVariantRequestHandler : IRequestHandler<CreateVariantRequest, Guid>
{
    private readonly IRepository<Product> _productRepository;

    public CreateVariantRequestHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateVariantRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(new ProductByIdSpec(request.ProductId), cancellationToken)
                ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        var variantFactory = VariantFactoryProvider.GetFactory(product.ProductType);
        var variant = variantFactory.CreateVariant(request);

        product.AddVariant(variant);

        return variant.Id;
    }
}

