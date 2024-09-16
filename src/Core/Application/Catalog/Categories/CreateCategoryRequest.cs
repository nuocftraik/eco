
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Application.Catalog.Categories;
public class CreateCategoryRequest : IRequest<Guid>
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> ProductIds { get; set; }
}

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters.");

        RuleFor(x => x.ProductIds)
            .Must(x => x == null || x.All(id => id != Guid.Empty)).WithMessage("ProductIds cannot contain empty GUID.");
    }
}


public class CreateCategoryRequestHandler : IRequestHandler<CreateCategoryRequest, Guid>
{
    private readonly IRepository<Category> _repository;

    public CreateCategoryRequestHandler(IRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name, request.Slug, request.IsActive);

        if (request.ProductIds != null && request.ProductIds.Any())
        {
            foreach (var productId in request.ProductIds)
            {
                category.AddProduct(productId); 
            }
        }
        await _repository.AddAsync(category, cancellationToken);

        return category.Id;
    }
}
