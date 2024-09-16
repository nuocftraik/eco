

using ECO.WebApi.Domain.Catalog;

namespace ECO.WebApi.Application.Catalog.Categories;
public class DeleteMutipleCategoryRequest : IRequest<List<Guid>>
{
    public List<Guid> Ids { get; set; }
}

//Handler
public class DeleteMutipleCategoryHandler : IRequestHandler<DeleteMutipleCategoryRequest, List<Guid>>
{
    private readonly IRepository<Category> _repository;
    private readonly IMediator _mediator;

    public DeleteMutipleCategoryHandler(IRepository<Category> repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<List<Guid>> Handle(DeleteMutipleCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryIds = new List<Guid>();
        foreach (var id in request.Ids)
        {
            await _mediator.Send(new DeleteCategoryRequest(id));
            categoryIds.Add(id);
        }
        return categoryIds;
    }
}



