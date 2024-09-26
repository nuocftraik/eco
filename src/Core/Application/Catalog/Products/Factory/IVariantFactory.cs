

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public interface IVariantFactory
{
    Variant CreateVariant(CreateVariantRequest request);
}
