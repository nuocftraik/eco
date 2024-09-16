

using ECO.WebApi.Application.Common.Persistence;
using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Application.Catalog.Categories;
public class DeleteCategoryRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteCategoryRequest(Guid id)
    {
        Id = id;
    }
}

public class DeleteCategoryRequestValidator : AbstractValidator<DeleteCategoryRequest>
{
    public DeleteCategoryRequestValidator()
    {
       
    }
}

//Handler
public class DeleteCategoryRequestHandler : IRequestHandler<DeleteCategoryRequest, Guid>
{
    private readonly IRepository<Category> _repository;

    public DeleteCategoryRequestHandler(IRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id)
              ?? throw new NotFoundException($"Category with ID {request.Id} was not found.");

        await _repository.DeleteAsync(category);
        return category.Id;
    }
}
