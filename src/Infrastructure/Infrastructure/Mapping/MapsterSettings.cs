using ECO.WebApi.Application.Catalog.Categories;
using ECO.WebApi.Domain.Catalog;
using Mapster;

namespace ECO.WebApi.Infrastructure.Mapping;
public class MapsterSettings
{
    public static void Configure()
    {
        // here we will define the type conversion / Custom-mapping
        // More details at https://github.com/MapsterMapper/Mapster/wiki/Custom-mapping

        // This one is actually not necessary as it's mapped by convention
        // TypeAdapterConfig<Product, ProductDto>.NewConfig().Map(dest => dest.BrandName, src => src.Brand.Name);

        //Category
        TypeAdapterConfig<Category, CategoryDto>.NewConfig()
            .Map(dest => dest.Products,src => src.ProductCategories.Select(x => x.Product.Adapt<ProductInCategoryDto>()).ToList());

        TypeAdapterConfig<Product, ProductInCategoryDto>.NewConfig()
            .Map(dest => dest.ProductId, src => src.Id) 
            .Map(dest => dest.ProductName, src => src.Name);

        TypeAdapterConfig<Category, CategoryInListDto>.NewConfig()
            .Map(dest => dest.NumberOfProduct, src => src.ProductCategories.Count);

    }
}
