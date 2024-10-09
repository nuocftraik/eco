

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public interface IVariantFactory : IScopedService
{
    Variant CreateVariant(CreateVariantRequest request);
}
