using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Enum;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Categories;
public class SearchCategoryRequest : PaginationFilter,IRequest<PaginationResponse<CategoryInListDto>>
{
    public DateRangeFilter PublishedFilter { get; set; }
    public DateRangeFilter CreatedFilter { get; set; }
    public DateRangeFilter ModifiedFilter { get; set; }
    public bool? IsActive { get; set; }
}

public class SearchCategoryRequestValidator : AbstractValidator<SearchCategoryRequest>
{
    public SearchCategoryRequestValidator()
    {
        // Validate DateRangeFilters
        RuleFor(x => x.PublishedFilter)
            .IsInEnum().WithMessage("Invalid value for Published DateRangeFilter.");

        RuleFor(x => x.CreatedFilter)
            .IsInEnum().WithMessage("Invalid value for Created DateRangeFilter.");

        RuleFor(x => x.ModifiedFilter)
            .IsInEnum().WithMessage("Invalid value for Modified DateRangeFilter.");
    }
}


//Handler
public class SearchCategoryRequestHandler : IRequestHandler<SearchCategoryRequest, PaginationResponse<CategoryInListDto>>
{
    private readonly IRepository<Category> _repository;

    public SearchCategoryRequestHandler(IRepository<Category> repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<CategoryInListDto>> Handle(SearchCategoryRequest request, CancellationToken cancellationToken)
    {
        var spec = new CategoryBySearchSpec(request);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);
        var data = await _repository.ListAsync(spec, cancellationToken);

        var dtoData = data.Adapt<List<CategoryInListDto>>();

        return new PaginationResponse<CategoryInListDto>(dtoData, totalCount, request.PageNumber, request.PageSize);
    }

}
