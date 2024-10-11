

namespace ECO.WebApi.Application.Catalog.Products.Factory;
public class SimpleProductVariantFactory : IVariantFactory
{
    public Variant CreateVariant(CreateVariantRequest request)
    {
        var variant = new Variant(request.ProductId, request.SKU, request.Price, request.MainImage, request.IsActive, request.IsDefault);

        // Áp dụng logic cho sản phẩm đơn giản (Simple Product)
        variant.TrackingBilling(request.Price, request.ComparePrice);
        variant.TrackingInventory(request.TrackInventory, request.Quantity);
        variant.TrackingShipping(request.RequireShipping, request.Weight, request.Width, request.Height, request.Length);
        variant.TrackingDownload(request.IncludeDownload, request.FileName, request.FileUrl);

        return variant;
    }
}

public class ConfigurableProductVariantFactory : IVariantFactory
{
    public Variant CreateVariant(CreateVariantRequest request)
    {
        var variant = new Variant(request.ProductId, request.SKU, request.Price, request.MainImage, request.IsActive, request.IsDefault);

        // Áp dụng logic cho sản phẩm đơn giản (Simple Product)
        variant.TrackingBilling(request.Price, request.ComparePrice);
        variant.TrackingInventory(request.TrackInventory, request.Quantity);
        variant.TrackingShipping(request.RequireShipping, request.Weight, request.Width, request.Height, request.Length);
        variant.TrackingDownload(request.IncludeDownload, request.FileName, request.FileUrl);

        variant.AddAttributeValue(request.AttributeValueIds);

        return variant;
    }
}
