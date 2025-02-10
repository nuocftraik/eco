

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

    public bool IsDefault { get; set; }


    public int? Quantity { get; set; }


    //Downloadable
    public bool IncludeDownload { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }

    public List<Guid>? AttributeValueIds { get; set; }

    public CreateVariantRequest(Guid productId, string? mainImage, double price, double? comparePrice, bool isDefault, int? quantity, bool includeDownload, string? fileName, string? fileUrl)
    {
        ProductId = productId;
        Status = ProductStatus.InStock;
        MainImage = mainImage;
        Price = price;
        ComparePrice = comparePrice;
        IsDefault = isDefault;
        Quantity = quantity;
        IncludeDownload = includeDownload;
        FileName = fileName;
        FileUrl = fileUrl;

    }
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
            .Must((model, comparePrice) => !comparePrice.HasValue || comparePrice.Value > model.Price)
            .WithMessage("ComparePrice must be greater than Price if provided.");

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
    private readonly IRepository<Variant> _variantRepository;
    public CreateVariantRequestHandler(IRepository<Product> productRepository, IRepository<Variant> variantRepository)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
    }

    public async Task<Guid> Handle(CreateVariantRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.FirstOrDefaultAsync(new ProductByIdSpec(request.ProductId), cancellationToken)
                ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        var variantFactory = VariantFactoryProvider.GetFactory(product.ProductType);
        var variant = variantFactory.CreateVariant(request);

        product.AddVariant(variant);
        await _variantRepository.AddAsync(variant, cancellationToken);

        return variant.Id;
    }
}

