using System.ComponentModel.DataAnnotations.Schema;


namespace ECO.WebApi.Domain.Catalog;
public class Category : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public bool IsActive { get; private set; }
    public virtual List<ProductCategory> ProductCategories { get; private set; }

    public Category(string name, string slug, bool isActive)
    {
        Name = name;
        Slug = slug;
        IsActive = isActive;
    }

    // Method để update thông tin Category
    public void Update(string name, string slug, bool isActive)
    {
        Name = name;
        Slug = slug;
        IsActive = isActive;
    }
    public void AddProduct(Guid productId)
    {
        ValidateId(productId);
        if (HasProduct(productId))
        {
            throw new InvalidOperationException($"Product with ID {productId} is already in this category.");
        }
        ProductCategories.Add(new ProductCategory(productId, Id));
    }

    public void UpdateProducts(List<Guid> newProductIds)
    {
        // Xóa các sản phẩm không có trong danh sách mới
        ProductCategories.RemoveAll(pc => !newProductIds.Contains(pc.ProductId));

        // Thêm các sản phẩm mới chưa có trong danh mục
        var existingProductIds = ProductCategories.Select(pc => pc.ProductId).ToHashSet(); // Chuyển sang HashSet để tối ưu hóa Contains
        foreach (var productId in newProductIds)
        {
            if (!existingProductIds.Contains(productId))
            {
                ProductCategories.Add(new ProductCategory(productId, Id));
            }
        }
    }




    private void ValidateId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }
    }

    private bool HasProduct(Guid productId)
    {
        return ProductCategories.Any(pc => pc.ProductId == productId);
    }





}
