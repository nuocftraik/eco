
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Application.Catalog.Categories;
public class UpdateCategoryRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public bool IsActive { get; set; }
    public List<Guid>? ProductIds { get; set; }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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

public class UpdateCategoryRequestHandler : IRequestHandler<UpdateCategoryRequest, Guid>
{
    private readonly IRepository<Category> _repository;

    public UpdateCategoryRequestHandler(IRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {


        var category = await _repository.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new NotFoundException($"Category with ID {request.Id} was not found.");

        category.Update(request.Name, request.Slug, request.IsActive);
        category.UpdateProducts(request.ProductIds ?? new List<Guid>());
        await _repository.UpdateAsync(category, cancellationToken);

        return category.Id;
    }
}
