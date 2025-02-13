using ECO.WebApi.Application.Payment;
using ECO.WebApi.Application.Payment.Models;
using Newtonsoft.Json;


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
    /// <returns></returns>
    [HttpPost("CreatePaymentUrl")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> CreatePaymentUrl(CreatePaymentRequest request)
    {
        var paymentUrl = await Mediator.Send(request);
        return Created(paymentUrl, paymentUrl);
    }

    /// <summary>
    /// Thực hiện hành động sau khi thanh toán. URL này cần được khai báo với VNPAY để API này hoạt đồng (ví dụ: https://localhost:5001/api/Vnpay/vnpay-ipn)
    /// </summary>
    /// <returns></returns>
    [HttpGet("vnpay-ipn")]
    [AllowAnonymous]

    public async Task<IActionResult> VnPayIpn()
    {
        if (Request.QueryString.HasValue)
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    // Thực hiện hành động nếu thanh toán thành công tại đây. Ví dụ: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu.
                    return Ok(new { Message = "Xử lý thành công" });
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

    public async Task<ActionResult<PaymentResult>> Callback()
    {
        if (!Request.QueryString.HasValue)
            return NotFound("Không tìm thấy thông tin thanh toán.");

        var command = new ProcessPaymentResultRequest(Request.Query);
        var result = await Mediator.Send(command);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

}
