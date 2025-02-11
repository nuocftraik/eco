namespace ECO.WebApi.Domain.Enum;

public enum PaymentMethod
{
    OnlinePayment = 0,
    COD = 1, // Cash on delivery : thanh toán khi nhận hàng
    Banking = 2,
    CreditCard = 3
}
