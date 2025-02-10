

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public class SimpleProductVariantFactory : IVariantFactory
{
    public Variant CreateVariant(CreateVariantRequest request)
    {
        var variant = new Variant(request.IsDefault, request.Quantity.Value, request.MainImage, request.Price, request.ComparePrice, request.IncludeDownload, request.FileName, request.FileUrl);

        // Áp dụng logic cho sản phẩm đơn giản (Simple Product)
        variant.TrackingBilling(request.Price, request.ComparePrice);
        variant.TrackingDownload(request.IncludeDownload, request.FileName, request.FileUrl);

        return variant;
    }
}

public class ConfigurableProductVariantFactory : IVariantFactory
{
    public Variant CreateVariant(CreateVariantRequest request)
    {
        var variant = new Variant(request.IsDefault, request.Quantity.Value,request.MainImage, request.Price,request.ComparePrice ,request.IncludeDownload, request.FileName, request.FileUrl);

        // Áp dụng logic cho sản phẩm đơn giản (Simple Product)
        variant.TrackingBilling(request.Price, request.ComparePrice);
        variant.TrackingDownload(request.IncludeDownload, request.FileName, request.FileUrl);
        //Logic cho configurable product
        variant.AddAttributeValue(request.AttributeValueIds);

        return variant;
    }
}
