using ECO.WebApi.Application.Catalog.Categories;
using ECO.WebApi.Application.Catalog.Products;
using ECO.WebApi.Application.Notifications;
using ECO.WebApi.Application.Payment.Models;
using ECO.WebApi.Domain.Attributes;
using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Payment;
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

        //Attribute 
        TypeAdapterConfig<Domain.Attributes.Attribute, AttributeDto>.NewConfig()
            .Map(dest => dest.AttributeValues, src => src.AttributeValues.Adapt<List<AttributeValueDto>>());

        TypeAdapterConfig<AttributeValue, AttributeValueDto>.NewConfig();

        //Product
        TypeAdapterConfig<Product, ProductDto>.NewConfig()
            .Map(dest => dest.Categories, src => src.ProductCategories.Select(x => x.Category.Adapt<CategoryInProductDto>()).ToList())
            .Map(dest => dest.Attributes, src => src.Attributes.Adapt<List<AttributeDto>>())
            .Map(dest => dest.Variants, src => src.Variants.Adapt<List<VariantDto>>());

        TypeAdapterConfig<Category, CategoryInProductDto>.NewConfig()
             .Map(dest => dest.CategoryId, src => src.Id)
            .Map(dest => dest.CategoryName, src => src.Name);

        TypeAdapterConfig<Product, ProductInListDto>.NewConfig();

        //Variant
        TypeAdapterConfig<Variant, VariantDto>.NewConfig()
            .Map(dest => dest.AttributeValues, src => src.VariantAttributeValues.Select(x => x.AttributeValue).Adapt<List<AttributeValueDto>>());


        //Payment

        TypeAdapterConfig<Payment, PaymentDto>.NewConfig();

        //Noti
        TypeAdapterConfig<Domain.Notifications.Notification, NotificationDto>.NewConfig();
    }
}
