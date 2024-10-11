
using ECO.WebApi.Domain.Enum;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Products;
public class SearchProductRequest : PaginationFilter, IRequest<PaginationResponse<ProductInListDto>>
{
    public DateRangeFilter PublishedFilter { get; set; }
    public DateRangeFilter CreatedFilter { get; set; }
    public DateRangeFilter ModifiedFilter { get; set; }
    public ProductType? ProductType { get; set; }
}

public class SearchProductRequestValidator : AbstractValidator<SearchProductRequest>
{
    public SearchProductRequestValidator()
    {
        // Validate DateRangeFilters
        RuleFor(x => x.PublishedFilter)
            .IsInEnum().WithMessage("Invalid value for Published DateRangeFilter.");

        RuleFor(x => x.CreatedFilter)
            .IsInEnum().WithMessage("Invalid value for Created DateRangeFilter.");

        RuleFor(x => x.ModifiedFilter)
            .IsInEnum().WithMessage("Invalid value for Modified DateRangeFilter.");

        RuleFor(x => x.ProductType)
            .IsInEnum().WithMessage("Invalid value for ProductType.");

    }
}

//Handler
public class SearchProductRequestHandler : IRequestHandler<SearchProductRequest, PaginationResponse<ProductInListDto>>
{
    private readonly IRepository<Product> _repository;

    public SearchProductRequestHandler(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<ProductInListDto>> Handle(SearchProductRequest request, CancellationToken cancellationToken)
    {
        var spec = new ProductBySearchSpec(request);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);
        var data = await _repository.ListAsync(spec, cancellationToken);

        var dtoData = data.Adapt<List<ProductInListDto>>();

        return new PaginationResponse<ProductInListDto>(dtoData, totalCount, request.PageNumber, request.PageSize);
    }

}
