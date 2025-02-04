

using ECO.WebApi.Infrastructure.VNPAY;
using ECO.WebApi.Infrastructure.VNPAY.Enums;
using ECO.WebApi.Infrastructure.VNPAY.Helpers;
using ECO.WebApi.Infrastructure.VNPAY.Models;

namespace ECO.WebApi.Host.Controllers.VNPAY;
public class VnPayController : BaseApiController
{
    private readonly IVnpay _vnpay;
    private readonly IConfiguration _configuration;

    public VnPayController(IVnpay vnpay, IConfiguration configuration)
    {
        _vnpay = vnpay;
        _configuration = configuration;
        _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);

    }
    /// <summary>
    /// Tạo url thanh toán
    /// </summary>
    /// <param name="money">Số tiền phải thanh toán</param>
    /// <param name="description">Mô tả giao dịch</param>
    /// <returns></returns>
    [HttpGet("CreatePaymentUrl")]
    [AllowAnonymous]
    public ActionResult<string> CreatePaymentUrl(double money, string description)
    {
        try
        {
            var ipAddress = NetworkHelper.GetIpAddress(HttpContext); // Lấy địa chỉ IP của thiết bị thực hiện giao dịch

            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = money,
                Description = description,
                IpAddress = ipAddress,
                BankCode = BankCode.ANY, // Tùy chọn. Mặc định là tất cả phương thức giao dịch
                CreatedDate = DateTime.Now, // Tùy chọn. Mặc định là thời điểm hiện tại
                Currency = Currency.VND, // Tùy chọn. Mặc định là VND (Việt Nam đồng)
                Language = DisplayLanguage.Vietnamese // Tùy chọn. Mặc định là tiếng Việt
            };

            var paymentUrl = _vnpay.GetPaymentUrl(request);

            return Created(paymentUrl, paymentUrl);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Thực hiện hành động sau khi thanh toán. URL này cần được khai báo với VNPAY để API này hoạt đồng (ví dụ: https://localhost:5001/api/Vnpay/vnpay-ipn)
    /// </summary>
    /// <returns></returns>
    [HttpGet("vnpay-ipn")]
    [AllowAnonymous]

    public IActionResult VnPayIpn()
    {
        if (Request.QueryString.HasValue)
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    // Thực hiện hành động nếu thanh toán thành công tại đây. Ví dụ: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu.
                    return Ok();
                }

                // Thực hiện hành động nếu thanh toán thất bại tại đây. Ví dụ: Hủy đơn hàng.
                return BadRequest("Thanh toán thất bại");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return NotFound("Không tìm thấy thông tin thanh toán.");
    }

    /// <summary>
    /// Trả kết quả thanh toán về cho người dùng
    /// </summary>
    /// <returns></returns>
    [HttpGet("Callback")]
    [AllowAnonymous]

    public ActionResult<PaymentResult> Callback()
    {
        if (Request.QueryString.HasValue)
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);

                if (paymentResult.IsSuccess)
                {
                    return Ok(paymentResult);
                }

                return BadRequest(paymentResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return NotFound("Không tìm thấy thông tin thanh toán.");
    }

}
