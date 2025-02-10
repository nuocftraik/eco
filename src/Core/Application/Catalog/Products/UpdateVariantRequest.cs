using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products;

public class UpdateVariantRequest : IRequest<Guid>
{
    public Guid Id { get; set; } // ID của variant cần cập nhật
    public Guid ProductId { get; set; } // Product liên quan đến variant này

    // Media
    public string? MainImage { get; set; }

    // Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }

    // Identifiers
    public bool IsDefault { get; set; }

    // Inventory
    public int? Quantity { get; set; }

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
        variant.Update(request.IsDefault,request.Price,request.MainImage,request.ComparePrice,request.IncludeDownload,request.Quantity,request.FileName, request.FileUrl);

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
