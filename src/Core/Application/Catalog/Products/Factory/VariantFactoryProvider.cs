

using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public class VariantFactoryProvider
{
    public static IVariantFactory GetFactory(ProductType productType)
    {
        return productType switch
        {
            ProductType.Simple => new SimpleProductVariantFactory(),
            ProductType.Grouped => new GroupedProductVariantFactory(),
            ProductType.Configurable => new ConfigurableProductVariantFactory(),
            //ProductType.Bundle => new BundleProductVariantFactory(),
            //ProductType.Virtual => new VirtualProductVariantFactory(),
            //ProductType.Downloadable => new DownloadableProductVariantFactory(),
            _ => throw new ArgumentException("Invalid product type", nameof(productType)),
        };
    }
}

