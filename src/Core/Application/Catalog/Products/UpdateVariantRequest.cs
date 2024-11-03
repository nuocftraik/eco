using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;

public class UpdateVariantRequest : IRequest<Guid>
{
    public Guid Id { get; set; } // ID của variant cần cập nhật
    public Guid ProductId { get; set; } // Product liên quan đến variant này
    public ProductStatus Status { get; set; }

    // Media
    public string? MainImage { get; set; }

    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    // Identifiers
    public string SKU { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }

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
    public List<Guid> AttributeValueIds { get; set; }
}

public class UpdateVariantRequestValidator : AbstractValidator<UpdateVariantRequest>
{
    public UpdateVariantRequestValidator()
    {
        // Id validation
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Variant ID is required.")
            .Must(x => x != Guid.Empty).WithMessage("Variant ID cannot be empty GUID.");

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

public class UpdateVariantRequestHandler : IRequestHandler<UpdateVariantRequest, Guid>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Variant> _variantRepository;
    public UpdateVariantRequestHandler(IRepository<Product> productRepository, IRepository<Variant> variantRepository)
    {
        _productRepository = productRepository;
        _variantRepository = variantRepository;
    }

    public async Task<Guid> Handle(UpdateVariantRequest request, CancellationToken cancellationToken)
    {
        // Lấy Product từ DB
        var product = await _productRepository.FirstOrDefaultAsync(new ProductByIdSpec(request.ProductId), cancellationToken)
                ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        // Lấy Variant cần cập nhật
        var variant = product.Variants.FirstOrDefault(v => v.Id == request.Id)
            ?? throw new NotFoundException($"Variant with ID {request.Id} was not found.");

        // Cập nhật các thuộc tính của variant
        variant.Update(
           request.SKU,
           request.Price,
           request.MainImage,
           request.IsActive,
           request.IsDefault,
           request.Status,
           request.TrackInventory,
           request.Quantity,
           request.RequireShipping,
           request.Weight,
           request.Width,
           request.Height,
           request.Length,
           request.IncludeDownload,
           request.FileName,
           request.FileUrl,
           request.ComparePrice
       );

        // Lấy tất cả các AttributeValueId của Product để kiểm tra
        var validAttributeValueIds = product.Attributes
            .SelectMany(a => a.AttributeValues.Select(av => av.Id))
            .ToHashSet();

        // Kiểm tra xem tất cả AttributeValueIds trong request có thuộc về Product hay không
        if (request.AttributeValueIds.Any(id => !validAttributeValueIds.Contains(id)))
        {
            throw new ValidationException("One or more attribute values are invalid for the given product.");
        }
        //variant.UpdateAttributeValues(request.AttributeValueIds);


        product.Attributes
       .SelectMany(attr => attr.AttributeValues)
       .ToList()
       .ForEach(av => av.VariantAttributeValues.Clear()); 


        foreach (var attributeValueId in request.AttributeValueIds)
        {
            variant.VariantAttributeValues.Add(new VariantAttributeValue(attributeValueId)); // Thêm mới
        }

        // Lưu thay đổi vào DB
        await _productRepository.UpdateAsync(product, cancellationToken);

        return variant.Id;
    }
}
