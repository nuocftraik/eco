using ECO.WebApi.Application.Catalog.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace ECO.WebApi.Host.Controllers.Catalog;
public class ProductController : BaseApiController
{

    //write controller for CreateProductRequest
    [HttpPost("create")]
    [OpenApiOperation("Create a new product.", "")]
    public Task<Guid> CreateAsync(CreateProductRequest request)
    {
        return Mediator.Send(request);
    }

    //write controller for UpdateProductRequest
    [HttpPut("update")]
    [OpenApiOperation("Update an existing product.", "")]
    public Task UpdateAsync(UpdateProductRequest request)
    {
        return Mediator.Send(request);
    }

    //write controller for DeleteProductRequest
    [HttpDelete("delete")]
    [OpenApiOperation("Delete an existing product.", "")]
    public Task DeleteAsync(DeleteProductRequest request)
    {
        return Mediator.Send(request);
    }

    //write controller for GetProductRequest
    [HttpGet("get")]
    [OpenApiOperation("Get a product by ID.", "")]
    public Task<ProductDto> GetAsync(Guid id)
    {
        return Mediator.Send(new GetProductRequest(id));
    }

    //write controller for CreateVariantRequest
    [HttpPost("create-variant")]
    [OpenApiOperation("Create a new variant for a product.", "")]
    public Task<Guid> CreateVariantAsync(CreateVariantRequest request)
    {
        return Mediator.Send(request);
    }

    //write controller for UpdateVariantRequest
    [HttpPut("update-variant")]
    [OpenApiOperation("Update an existing variant of a product.", "")]
    public Task UpdateVariantAsync(UpdateVariantRequest request)
    {
        return Mediator.Send(request);
    }   

    //write controller for DeleteVariantRequest
    [HttpDelete("delete-variant")]
    [OpenApiOperation("Delete an existing variant of a product.", "")]
    public Task DeleteVariantAsync(DeleteVariantRequest request)
    {
        return Mediator.Send(request);
    }
    
}
