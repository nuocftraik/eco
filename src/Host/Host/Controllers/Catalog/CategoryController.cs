
using ECO.WebApi.Application.Catalog.Categories;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Catalog;
public class CategoryController : BaseApiController
{
    [HttpGet("get")]
    [OpenApiOperation("Get a category by ID.", "")]
    public Task<CategoryDto> GetAsync(Guid id)
    {
        return Mediator.Send(new GetCategoryRequest(id));
    }

    [HttpPost("search")]
    [OpenApiOperation("Search categories using available filters.", "")]
    public Task<PaginationResponse<CategoryInListDto>> SearchAsync(SearchCategoryRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpGet("get-available-categories-of-product")]
    [OpenApiOperation("Get available categories of a product.", "")]
    public Task<List<AvailableCategoryDto>> GetAvailableCategoriesOfProductAsync(Guid productId)
    {
        return Mediator.Send(new GetAvailableCategoryOfProductRequest(productId));
    }

    [HttpPost("create")]
    [OpenApiOperation("Create a new category.", "")]
    public Task<Guid> CreateAsync(CreateCategoryRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPut("update")]
    [OpenApiOperation("Update an existing category.", "")]
    public Task UpdateAsync(UpdateCategoryRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpDelete("delete")]
    [OpenApiOperation("Delete an existing category.", "")]
    public Task DeleteAsync(DeleteCategoryRequest request)
    {
        return Mediator.Send(request);
    }



}
