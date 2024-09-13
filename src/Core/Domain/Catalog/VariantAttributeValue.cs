using ECO.WebApi.Domain.Attributes;
using Microsoft.EntityFrameworkCore;



namespace ECO.WebApi.Domain.Catalog;
[PrimaryKey(nameof(VariantId), nameof(AttributeValueId))]
public class VariantAttributeValue
{
    public Guid VariantId { get; set; }
    public Guid AttributeValueId { get; set; }
    public virtual Attributes.AttributeValue AttributeValue { get; set; }
    public virtual Variant Variant { get; set; }
}


//Cụ thể, khi bạn xóa một Product, hệ thống sẽ thực hiện xóa như sau:

//1. Đường dẫn 1:
//Xóa Product sẽ dẫn đến việc xóa tất cả các Variant liên quan.
//Khi Variant bị xóa, nó sẽ xóa các bản ghi liên quan trong VariantAttributeValues do hành vi cascade.
//2. Đường dẫn 2:
//Xóa Product cũng sẽ dẫn đến việc xóa tất cả các Attribute liên quan.
//Khi các Attribute bị xóa, tất cả các AttributeValue liên quan cũng sẽ bị xóa.
//Cuối cùng, khi AttributeValue bị xóa, nó sẽ lại tác động đến VariantAttributeValues và xóa bản ghi liên quan.
//Vấn đề (Multiple Cascade Paths):
//Cả hai đường dẫn đều cố gắng xóa dữ liệu từ bảng VariantAttributeValues:

//Một đường từ Variant đến VariantAttributeValues.
//Một đường từ AttributeValue đến VariantAttributeValues.
//Vì vậy, SQL Server phát hiện ra rằng có hai đường dẫn xóa cascade độc lập nhưng cùng hướng đến bảng VariantAttributeValues, dẫn đến lỗi multiple cascade paths.
