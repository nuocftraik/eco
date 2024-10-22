
using ECO.WebApi.Application.Catalog.Products.Dtos;
using ECO.WebApi.Application.Catalog.Products.Factory;
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

    // Thêm các trường cho Variant nếu productType là Simple
   
    public string? SKU { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    //Billing
    public double Price { get; set; }
    public double? ComparePrice { get; set; }
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
    private readonly IVariantFactory _variantFactory;
    public CreateProductRequestHandler(IRepository<Product> productRepository, IVariantFactory variantFactory)
    {
        _productRepository = productRepository;
        _variantFactory = variantFactory;
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

        // Nếu là Simple Product thì tạo một variant mặc định
        if (request.ProductType == ProductType.Simple)
        {
            // Sử dụng CreateVariantRequest để tạo variant cho Simple Product
            var variantRequest = new CreateVariantRequest(product.Id,product.Status, request.MainImage, request.Price, request.ComparePrice,  request.SKU, request.IsActive, request.IsDefault, request.TrackInventory, request.Quantity, request.RequireShipping, request.Weight, request.Width, request.Height, request.Length, request.IncludeDownload, request.FileName, request.FileUrl);

            // Tạo Variant từ Factory
            var variantFactory = VariantFactoryProvider.GetFactory(ProductType.Simple);
            var defaultVariant = variantFactory.CreateVariant(variantRequest);
            product.AddVariant(defaultVariant);
        }

        return product.Id;
    }
}


