using Mapster;

namespace ECO.WebApi.Application.Catalog.Categories;
public class GetAvailableCategoryOfProductRequest : IRequest<List<AvailableCategoryDto>>
{
    public Guid ProductId { get; set; }

    public GetAvailableCategoryOfProductRequest(Guid productId)
    {
        ProductId = productId;
    }
}

//Handler
public class GetAvailableCategoryRequestHandler : IRequestHandler<GetAvailableCategoryOfProductRequest, List<AvailableCategoryDto>>
{
    private readonly IRepository<Category> _repository;
    private readonly IRepository<Product> _repositoryProduct;

    public GetAvailableCategoryRequestHandler(IRepository<Category> repository, IRepository<Product> repositoryProduct)
    {
        _repository = repository;
        _repositoryProduct = repositoryProduct;
    }

    public async Task<List<AvailableCategoryDto>> Handle(GetAvailableCategoryOfProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repositoryProduct.FirstOrDefaultAsync(new ProductByIdWithCategorySpec(request.ProductId),cancellationToken)
            ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        var categoryIds = product.ProductCategories.Select(x => x.CategoryId).ToList();

        var availableCategories = await _repository.ListAsync(new AvailableCategoryOfProductSpec(categoryIds), cancellationToken);

        var dtoData = availableCategories.Adapt<List<AvailableCategoryDto>>();

        return dtoData;
    }
}
