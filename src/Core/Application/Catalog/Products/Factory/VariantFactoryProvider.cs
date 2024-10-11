

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public class VariantFactoryProvider
{
    public static IVariantFactory GetFactory(ProductType productType)
    {
        return productType switch
        {
            ProductType.Simple => new SimpleProductVariantFactory(),
            ProductType.Configurable => new ConfigurableProductVariantFactory(),
            _ => throw new ArgumentException("Invalid product type", nameof(productType)),
        };
    }
}

