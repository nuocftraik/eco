# BUILD_35: Payment Gateway Integration - VNPay (Bank Transfer)

> 📚 [Quay lại Mục lục](BUILD_INDEX.md)  
> 📋 **Prerequisites:** BUILD_34 (Payment Gateway Database Design) đã complete

Tài liệu này hướng dẫn **tích hợp VNPay Payment Gateway** với focus vào **chuyển khoản ngân hàng** (Bank Transfer) - phương thức phổ biến nhất tại Việt Nam.

---

## 1. Overview

**Làm gì:** Tích hợp VNPay Payment Gateway để xử lý thanh toán qua chuyển khoản ngân hàng.

**Tại sao cần:**
- **VNPay là #1 tại VN:** Kết nối với 50+ ngân hàng Việt Nam
- **Bank Transfer:** Phương thức phổ biến nhất (80% giao dịch online VN)
- **Secure:** PCI-DSS compliant, không lưu thông tin thẻ
- **Real-time:** Webhook callback tức thì khi khách hàng thanh toán
- **Vietnamese Market:** Hỗ trợ VND, giao diện tiếng Việt

**Trong bước này chúng ta sẽ:**
- ✅ Setup VNPay configuration (TmnCode, HashSecret, URLs)
- ✅ Implement VNPayService (CreatePaymentUrl, ValidateCallback)
- ✅ Tạo PaymentRequest DTOs
- ✅ Implement Webhook handler (IPN - Instant Payment Notification)
- ✅ Integrate với Order workflow (BUILD_32)
- ✅ Security: Hash validation, Idempotency
- ✅ Testing với VNPay Sandbox
- ✅ Payment Controllers & Specifications
- ✅ Complete end-to-end testing guide

**Real-world workflow:**
```csharp
// 1. Customer clicks "Thanh toán" button
var order = await CreateOrderAsync(cartId);

// 2. Generate VNPay payment URL
var paymentUrl = await _vnpayService.CreatePaymentUrlAsync(order);

// 3. Redirect customer to VNPay
return Redirect(paymentUrl);

// 4. Customer completes payment on VNPay
// → VNPay sends webhook callback

// 5. Process webhook
await _vnpayService.ProcessCallbackAsync(webhookData);

// 6. Update order status → Confirm order → Send email
```

---

## 2. Add Required Packages

### Bước 2.1: Add Required NuGet Packages

**File:** `src/Infrastructure/Infrastructure/Infrastructure.csproj`

```xml
<ItemGroup>
    <!-- VNPay không có official package, chúng ta sẽ implement service từ docs -->
    <!-- Packages cần thiết cho payment processing -->
    <PackageReference Include="System.Security.Cryptography.Algorithms" Version="4.3.1" />
</ItemGroup>
```

**Giải thích:**
- `System.Security.Cryptography.Algorithms`: For HMACSHA512 hash validation (VNPay yêu cầu)
- VNPay không có official NuGet package, chúng ta implement service theo [VNPay API Documentation](https://sandbox.vnpayment.vn/apis/)

---

## 3. VNPay Configuration

### Bước 3.1: VNPaySettings Configuration Class

**Làm gì:** Tạo settings class để lưu VNPay credentials.

**Tại sao:** Tách config ra khỏi code, dễ thay đổi giữa Sandbox và Production.

**File:** `src/Infrastructure/Infrastructure/Payment/VNPay/VNPaySettings.cs`

```csharp
namespace ECO.WebApi.Infrastructure.Payment.VNPay;

/// <summary>
/// VNPay payment gateway settings
/// </summary>
public class VNPaySettings
{
    /// <summary>
    /// Terminal Code (TmnCode) - provided by VNPay
    /// </summary>
    public string TmnCode { get; set; } = default!;
    
    /// <summary>
    /// Hash Secret Key - for signature validation
    /// </summary>
    public string HashSecret { get; set; } = default!;
 
 /// <summary>
    /// VNPay Payment Gateway URL
    /// Sandbox: https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
    /// Production: https://vnpayment.vn/paymentv2/vpcpay.html
    /// </summary>
    public string BaseUrl { get; set; } = default!;
    
    /// <summary>
    /// Return URL - where VNPay redirects after payment
    /// </summary>
    public string ReturnUrl { get; set; } = default!;
  
    /// <summary>
 /// IPN URL (Webhook) - VNPay calls this to notify payment result
    /// Must be publicly accessible
    /// </summary>
    public string IpnUrl { get; set; } = default!;
    
    /// <summary>
    /// API Version (default: 2.1.0)
    /// </summary>
    public string Version { get; set; } = "2.1.0";
    
    /// <summary>
    /// Command (default: pay)
    /// </summary>
  public string Command { get; set; } = "pay";
    
    /// <summary>
    /// Currency Code (VND only)
    /// </summary>
    public string CurrencyCode { get; set; } = "VND";
    
    /// <summary>
    /// Locale (vn or en)
    /// </summary>
    public string Locale { get; set; } = "vn";
}
```

**Giải thích:**
- **TmnCode & HashSecret:** Credentials từ VNPay (đăng ký tại [sandbox.vnpayment.vn](https://sandbox.vnpayment.vn))
- **BaseUrl:** Sandbox cho testing, Production khi go-live
- **ReturnUrl:** URL trong app của bạn để VNPay redirect sau khi khách thanh toán (UI endpoint)
- **IpnUrl:** Webhook URL để VNPay gọi server-to-server (API endpoint, phải public)
- **Version:** API version của VNPay (hiện tại: 2.1.0)

---

### Bước 3.2: Add Configuration to appsettings.json

**Làm gì:** Configure VNPay settings trong appsettings.

**File:** `src/Host/Host/Configurations/payment.json` (hoặc `appsettings.Development.json`)

```json
{
  "VNPaySettings": {
    "TmnCode": "YOUR_TMNCODE_HERE",
    "HashSecret": "YOUR_HASHSECRET_HERE",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:7001/api/payment/vnpay/callback",
    "IpnUrl": "https://your-domain.com/api/payment/vnpay/ipn",
    "Version": "2.1.0",
    "Command": "pay",
    "CurrencyCode": "VND",
    "Locale": "vn"
  }
}
```

**⚠️ Lưu ý:**
- **Sandbox credentials:** Đăng ký tại https://sandbox.vnpayment.vn/merchant_webapi/merchant.html
- **IpnUrl:** Phải là URL public (dùng ngrok cho local testing: `ngrok http 7001`)
- **Production:** Thay `BaseUrl`, `TmnCode`, `HashSecret` khi deploy

**Test credentials (VNPay Sandbox):**
```
TmnCode: Liên hệ VNPay để lấy
HashSecret: Liên hệ VNPay để lấy
Test Card: 9704198526191432198
Card Name: NGUYEN VAN A
Issue Date: 07/15
OTP: 123456 (Sandbox OTP cố định)
```

---

## 4. VNPay Service Implementation

### Bước 4.1: IVNPayService Interface

**Làm gì:** Define interface cho VNPay service.

**Tại sao:** Abstraction để dễ test và swap implementation.

**File:** `src/Core/Application/Common/Payment/IVNPayService.cs`

```csharp
namespace ECO.WebApi.Application.Common.Payment;

/// <summary>
/// VNPay payment gateway service
/// </summary>
public interface IVNPayService
{
    /// <summary>
    /// Create VNPay payment URL
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="amount">Payment amount (VND)</param>
    /// <param name="orderInfo">Order description</param>
    /// <param name="ipAddress">Customer IP address</param>
    /// <returns>VNPay payment URL</returns>
    Task<string> CreatePaymentUrlAsync(
  Guid orderId,
        decimal amount,
        string orderInfo,
     string ipAddress,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate and process VNPay callback (Return URL)
    /// </summary>
    /// <param name="queryParams">Query parameters from VNPay</param>
    /// <returns>Payment result</returns>
    Task<VNPayCallbackResult> ProcessReturnCallbackAsync(
        Dictionary<string, string> queryParams,
      CancellationToken cancellationToken = default);
    
 /// <summary>
  /// Process VNPay IPN (Webhook) - Server-to-server notification
    /// </summary>
    /// <param name="queryParams">Query parameters from VNPay</param>
    /// <returns>IPN response</returns>
    Task<VNPayIpnResponse> ProcessIpnAsync(
        Dictionary<string, string> queryParams,
  CancellationToken cancellationToken = default);
}

/// <summary>
/// VNPay callback result
/// </summary>
public record VNPayCallbackResult(
    bool Success,
    string TransactionId,
    Guid OrderId,
    decimal Amount,
    string? Message = null);

/// <summary>
/// VNPay IPN response
/// </summary>
public record VNPayIpnResponse(
    string RspCode,
    string Message);
```

**Giải thích:**
- **CreatePaymentUrlAsync:** Generate URL redirect đến VNPay
- **ProcessReturnCallbackAsync:** Xử lý khi VNPay redirect về (user flow)
- **ProcessIpnAsync:** Xử lý webhook từ VNPay (server-to-server, quan trọng nhất)

---

### Bước 4.2: VNPayService Implementation (FULL CODE) ⭐⭐⭐

**Làm gì:** Implement VNPay service với hash validation và URL generation.

**Tại sao:** Core logic để tích hợp VNPay.

**File:** `src/Infrastructure/Infrastructure/Payment/VNPay/VNPayService.cs`

```csharp
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Application.Common.Payment;
using ECO.WebApi.Domain.Enum;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECO.WebApi.Infrastructure.Payment.VNPay;

/// <summary>
/// VNPay payment gateway service implementation
/// </summary>
public class VNPayService : IVNPayService
{
 private readonly VNPaySettings _settings;
    private readonly IRepository<Domain.Payment.PaymentTransaction> _paymentRepository;
    private readonly IRepository<Domain.Payment.PaymentWebhook> _webhookRepository;
    private readonly IRepository<Domain.Order.Order> _orderRepository;
    private readonly ILogger<VNPayService> _logger;
    
    public VNPayService(
        IOptions<VNPaySettings> settings,
        IRepository<Domain.Payment.PaymentTransaction> paymentRepository,
IRepository<Domain.Payment.PaymentWebhook> webhookRepository,
        IRepository<Domain.Order.Order> orderRepository,
      ILogger<VNPayService> logger)
    {
        _settings = settings.Value;
        _paymentRepository = paymentRepository;
        _webhookRepository = webhookRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }
    
    // ==================== CREATE PAYMENT URL ⭐ ====================
    
    public async Task<string> CreatePaymentUrlAsync(
        Guid orderId,
        decimal amount,
        string orderInfo,
        string ipAddress,
        CancellationToken cancellationToken = default)
  {
        // 1. Get order to validate
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
 throw new NotFoundException($"Order {orderId} not found");
        
        // 2. Create payment transaction record
      var transaction = new Domain.Payment.PaymentTransaction(
        orderId: orderId,
     paymentMethod: Domain.Enum.PaymentMethod.VNPay,
        amount: amount,
            currency: "VND",
            paymentProviderId: null, // Set sau khi có PaymentProvider entity
     expirationPeriod: TimeSpan.FromMinutes(15) // VNPay payment expires after 15 minutes
     );
        
      await _paymentRepository.AddAsync(transaction, cancellationToken);
        
  // 3. Build VNPay request parameters
        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", _settings.Version },
        { "vnp_Command", _settings.Command },
            { "vnp_TmnCode", _settings.TmnCode },
            { "vnp_Amount", ((long)(amount * 100)).ToString() }, // VNPay yêu cầu amount * 100 (VND không có xu)
      { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", _settings.CurrencyCode },
     { "vnp_IpAddr", ipAddress },
    { "vnp_Locale", _settings.Locale },
            { "vnp_OrderInfo", orderInfo },
    { "vnp_OrderType", "other" }, // topup, billpayment, other
         { "vnp_ReturnUrl", _settings.ReturnUrl },
     { "vnp_TxnRef", transaction.IdempotencyKey }, // Unique transaction reference
            { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") }
    };
        
        // 4. Build query string
        var queryString = BuildQueryString(vnpParams);
        
        // 5. Generate secure hash (HMACSHA512)
        var secureHash = GenerateSecureHash(queryString, _settings.HashSecret);
      
      // 6. Build final payment URL
        var paymentUrl = $"{_settings.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        
        // 7. Store raw request for debugging
        transaction.StoreRawRequest(System.Text.Json.JsonSerializer.Serialize(vnpParams));
        await _paymentRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Created VNPay payment URL for Order {OrderId}, Transaction {TransactionId}, Amount {Amount}",
            orderId, transaction.IdempotencyKey, amount);
   
    return paymentUrl;
  }
    
    // ==================== PROCESS RETURN CALLBACK (User Flow) ====================
    
    public async Task<VNPayCallbackResult> ProcessReturnCallbackAsync(
        Dictionary<string, string> queryParams,
      CancellationToken cancellationToken = default)
    {
// 1. Validate secure hash
        if (!ValidateSecureHash(queryParams, _settings.HashSecret))
     {
       _logger.LogWarning("VNPay return callback: Invalid secure hash");
     return new VNPayCallbackResult(
          Success: false,
       TransactionId: "",
     OrderId: Guid.Empty,
        Amount: 0,
     Message: "Invalid signature");
        }
        
   // 2. Extract parameters
        var vnpTxnRef = queryParams["vnp_TxnRef"]; // Our IdempotencyKey
      var vnpTransactionNo = queryParams["vnp_TransactionNo"]; // VNPay transaction ID
  var vnpResponseCode = queryParams["vnp_ResponseCode"]; // "00" = success
   var vnpAmount = long.Parse(queryParams["vnp_Amount"]) / 100m; // Convert back to VND
 
        // 3. Find payment transaction
     var transaction = await _paymentRepository
         .FirstOrDefaultAsync(new PaymentTransactionByIdempotencyKeySpec(vnpTxnRef), cancellationToken);
        
    if (transaction == null)
        {
            _logger.LogWarning("VNPay return callback: Transaction {TxnRef} not found", vnpTxnRef);
 return new VNPayCallbackResult(
     Success: false,
   TransactionId: vnpTransactionNo,
          OrderId: Guid.Empty,
       Amount: vnpAmount,
       Message: "Transaction not found");
        }
        
        // 4. Check if already processed (idempotency)
        if (transaction.Status == PaymentStatus.Completed)
        {
       _logger.LogInformation("VNPay return callback: Transaction {TxnRef} already processed", vnpTxnRef);
            return new VNPayCallbackResult(
   Success: true,
   TransactionId: vnpTransactionNo,
 OrderId: transaction.OrderId,
      Amount: vnpAmount,
      Message: "Already processed");
        }
        
   // 5. Process payment result
        if (vnpResponseCode == "00") // Success
      {
        transaction.SetTransactionId(vnpTransactionNo);
            transaction.SetCompleted(System.Text.Json.JsonSerializer.Serialize(queryParams));
            
            _logger.LogInformation(
    "VNPay payment SUCCESS: Order {OrderId}, TransactionId {TransactionId}, Amount {Amount}",
          transaction.OrderId, vnpTransactionNo, vnpAmount);
    
         // Get order and confirm payment
       var order = await _orderRepository.GetByIdAsync(transaction.OrderId, cancellationToken);
            order.ConfirmPayment(); // This raises OrderConfirmedEvent → Deduct inventory
  
            await _paymentRepository.SaveChangesAsync(cancellationToken);
          
     return new VNPayCallbackResult(
                Success: true,
     TransactionId: vnpTransactionNo,
       OrderId: transaction.OrderId,
       Amount: vnpAmount,
          Message: "Payment successful");
        }
        else // Failed
        {
  transaction.SetFailed(System.Text.Json.JsonSerializer.Serialize(queryParams));
         
            _logger.LogWarning(
         "VNPay payment FAILED: Order {OrderId}, ResponseCode {ResponseCode}",
    transaction.OrderId, vnpResponseCode);
  
         await _paymentRepository.SaveChangesAsync(cancellationToken);
          
     return new VNPayCallbackResult(
     Success: false,
       TransactionId: vnpTransactionNo,
       OrderId: transaction.OrderId,
        Amount: vnpAmount,
          Message: GetVNPayResponseMessage(vnpResponseCode));
        }
    }
    
    // ==================== PROCESS IPN (Webhook - Most Important) ⭐⭐⭐ ====================
    
    public async Task<VNPayIpnResponse> ProcessIpnAsync(
        Dictionary<string, string> queryParams,
        CancellationToken cancellationToken = default)
    {
    // 1. Log webhook received
        _logger.LogInformation("VNPay IPN received: {Params}", 
      System.Text.Json.JsonSerializer.Serialize(queryParams));
        
   // 2. Create webhook record
   var webhook = new Domain.Payment.PaymentWebhook(
       paymentProviderId: null, // Set sau
   eventType: "payment.completed",
payload: System.Text.Json.JsonSerializer.Serialize(queryParams),
 signature: queryParams.GetValueOrDefault("vnp_SecureHash") ?? "");
     
        await _webhookRepository.AddAsync(webhook, cancellationToken);
        
        // 3. Validate secure hash
        if (!ValidateSecureHash(queryParams, _settings.HashSecret))
   {
        _logger.LogWarning("VNPay IPN: Invalid secure hash");
      webhook.MarkAsVerified(false);
            await _webhookRepository.SaveChangesAsync(cancellationToken);
         
      return new VNPayIpnResponse(
    RspCode: "97", // Invalid signature
Message: "Invalid signature");
        }
        
        webhook.MarkAsVerified(true);
        
   // 4. Extract parameters
        var vnpTxnRef = queryParams["vnp_TxnRef"];
        var vnpTransactionNo = queryParams["vnp_TransactionNo"];
   var vnpResponseCode = queryParams["vnp_ResponseCode"];
        var vnpAmount = long.Parse(queryParams["vnp_Amount"]) / 100m;
        
        // 5. Find payment transaction
     var transaction = await _paymentRepository
            .FirstOrDefaultAsync(new PaymentTransactionByIdempotencyKeySpec(vnpTxnRef), cancellationToken);
        
 if (transaction == null)
        {
        _logger.LogWarning("VNPay IPN: Transaction {TxnRef} not found", vnpTxnRef);
            webhook.MarkAsProcessed(false, "Transaction not found");
            await _webhookRepository.SaveChangesAsync(cancellationToken);
      
         return new VNPayIpnResponse(
       RspCode: "01", // Order not found
                Message: "Order not found");
        }
      
        // 6. Check amount match
        if (transaction.Amount != vnpAmount)
        {
            _logger.LogWarning(
              "VNPay IPN: Amount mismatch. Expected {Expected}, Received {Received}",
 transaction.Amount, vnpAmount);
            
            webhook.MarkAsProcessed(false, "Amount mismatch");
          await _webhookRepository.SaveChangesAsync(cancellationToken);
     
   return new VNPayIpnResponse(
         RspCode: "04", // Amount mismatch
          Message: "Amount mismatch");
        }
 
        // 7. Check if already processed (idempotency)
  if (transaction.Status == PaymentStatus.Completed)
        {
    _logger.LogInformation("VNPay IPN: Transaction {TxnRef} already processed", vnpTxnRef);
            webhook.MarkAsProcessed(true, "Already processed");
 await _webhookRepository.SaveChangesAsync(cancellationToken);
      
     return new VNPayIpnResponse(
       RspCode: "00", // Success
             Message: "Confirm Success");
      }
   
    // 8. Process payment result
     if (vnpResponseCode == "00") // Success
        {
            transaction.SetTransactionId(vnpTransactionNo);
            transaction.SetCompleted(System.Text.Json.JsonSerializer.Serialize(queryParams));
       
       // Confirm order
  var order = await _orderRepository.GetByIdAsync(transaction.OrderId, cancellationToken);
            order.ConfirmPayment(); // Raises OrderConfirmedEvent → Inventory deduction, email, etc.
            
            webhook.MarkAsProcessed(true, "Payment confirmed");
            await _paymentRepository.SaveChangesAsync(cancellationToken);
            
      _logger.LogInformation(
     "VNPay IPN processed SUCCESS: Order {OrderId}, TransactionId {TransactionId}",
             transaction.OrderId, vnpTransactionNo);
         
    return new VNPayIpnResponse(
      RspCode: "00", // Success
        Message: "Confirm Success");
        }
      else // Failed
        {
   transaction.SetFailed(System.Text.Json.JsonSerializer.Serialize(queryParams));
    
     webhook.MarkAsProcessed(true, $"Payment failed: {vnpResponseCode}");
  await _paymentRepository.SaveChangesAsync(cancellationToken);
   
     _logger.LogWarning(
            "VNPay IPN processed FAILED: Order {OrderId}, ResponseCode {ResponseCode}",
     transaction.OrderId, vnpResponseCode);
      
    return new VNPayIpnResponse(
         RspCode: "00", // Still return success to VNPay (we processed the webhook)
    Message: "Confirm Success");
 }
    }
    
    // ==================== HELPER METHODS ====================
    
  /// <summary>
    /// Build query string from sorted parameters
    /// </summary>
    private string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        var queryString = string.Join("&", 
         parameters.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));
        
    return queryString;
    }
    
    /// <summary>
    /// Generate HMACSHA512 secure hash
    /// </summary>
    private string GenerateSecureHash(string data, string hashSecret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(hashSecret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
    
        using var hmac = new HMACSHA512(keyBytes);
    var hashBytes = hmac.ComputeHash(dataBytes);
  
        return BitConverter.ToString(hashBytes)
 .Replace("-", "")
      .ToLower();
    }
    
    /// <summary>
    /// Validate VNPay secure hash from callback
    /// </summary>
    private bool ValidateSecureHash(Dictionary<string, string> queryParams, string hashSecret)
    {
        // 1. Extract secure hash from params
     if (!queryParams.TryGetValue("vnp_SecureHash", out var receivedHash))
     return false;
     
        // 2. Remove hash from params
    var paramsToValidate = new SortedDictionary<string, string>(queryParams);
        paramsToValidate.Remove("vnp_SecureHash");
paramsToValidate.Remove("vnp_SecureHashType");
        
        // 3. Build query string
        var queryString = BuildQueryString(paramsToValidate);
  
    // 4. Generate expected hash
        var expectedHash = GenerateSecureHash(queryString, hashSecret);
        
        // 5. Compare
     return string.Equals(receivedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Get user-friendly message for VNPay response code
    /// </summary>
    private string GetVNPayResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
       "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
  "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
 "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
      "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
  "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
"75" => "Ngân hàng thanh toán đang bảo trì.",
        "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
    _ => $"Giao dịch không thành công. Mã lỗi: {responseCode}"
        };
    }
}
```

**Giải thích chi tiết:**

**CreatePaymentUrlAsync:**
- Build sorted dictionary với tất cả VNPay params
- Amount * 100 (VNPay yêu cầu format không có dấu phẩy)
- Generate HMACSHA512 hash với HashSecret
- Return URL để redirect customer

**ProcessReturnCallbackAsync:**
- Validate hash signature
- Find transaction by IdempotencyKey
- Check idempotency (prevent double processing)
- Update transaction status
- Confirm order nếu success

**ProcessIpnAsync (QUAN TRỌNG NHẤT):**
- VNPay gọi API này sau khi customer thanh toán
- Server-to-server call, không qua browser
- Validate hash, amount, idempotency
- Update transaction + Order status
- Return "00" để VNPay biết đã nhận webhook

**Security:**
- ✅ HMACSHA512 hash validation
- ✅ Idempotency check (prevent duplicate processing)
- ✅ Amount validation
- ✅ Transaction status check

---

## 5. Payment Specifications

### Bước 5.1: PaymentTransactionByIdempotencyKeySpec

**Làm gì:** Specification để query PaymentTransaction by IdempotencyKey.

**Tại sao:** Cần find transaction nhanh chóng khi xử lý callback/webhook.

**File:** `src/Core/Application/Payment/Specifications/PaymentTransactionByIdempotencyKeySpec.cs`

```csharp
using Ardalis.Specification;
using ECO.WebApi.Domain.Payment;

namespace ECO.WebApi.Application.Payment.Specifications;

/// <summary>
/// Specification to find PaymentTransaction by IdempotencyKey
/// </summary>
public class PaymentTransactionByIdempotencyKeySpec : Specification<PaymentTransaction>
{
    public PaymentTransactionByIdempotencyKeySpec(string idempotencyKey)
    {
  Query.Where(pt => pt.IdempotencyKey == idempotencyKey);
 }
}
```

---

### Bước 5.2: PaymentTransactionByOrderIdSpec

**File:** `src/Core/Application/Payment/Specifications/PaymentTransactionByOrderIdSpec.cs`

```csharp
using Ardalis.Specification;
using ECO.WebApi.Domain.Payment;

namespace ECO.WebApi.Application.Payment.Specifications;

/// <summary>
/// Specification to find PaymentTransactions by OrderId
/// </summary>
public class PaymentTransactionByOrderIdSpec : Specification<PaymentTransaction>
{
    public PaymentTransactionByOrderIdSpec(Guid orderId)
  {
     Query
            .Where(pt => pt.OrderId == orderId)
       .OrderByDescending(pt => pt.CreatedAt);
    }
}
```

---

## 6. Payment Controllers

### Bước 6.1: PaymentController - Complete Implementation

**Làm gì:** API endpoints để tạo payment URL, xử lý callback và IPN.

**File:** `src/Host/Host/Controllers/Payment/PaymentController.cs`

```csharp
using ECO.WebApi.Application.Common.Payment;
using ECO.WebApi.Domain.Order;
using ECO.WebApi.Infrastructure.Auth.Permissions;
using ECO.WebApi.Shared.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECO.WebApi.Host.Controllers.Payment;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly IVNPayService _vnpayService;
  private readonly IRepository<Order> _orderRepository;
    private readonly ILogger<PaymentController> _logger;
    
    public PaymentController(
    IVNPayService vnpayService,
      IRepository<Order> orderRepository,
        ILogger<PaymentController> logger)
    {
 _vnpayService = vnpayService;
     _orderRepository = orderRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Create VNPay payment URL and redirect
    /// </summary>
    [HttpPost("vnpay/create")]
    [MustHavePermission(ECOAction.Create, ECOFunction.Orders)]
    [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreatePaymentResponse>> CreateVNPayPayment(
        [FromBody] CreatePaymentRequest request,
      CancellationToken cancellationToken)
    {
 // 1. Validate order exists and belongs to current user
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
       return NotFound(new { Message = $"Order {request.OrderId} not found" });
      }
        
      // 2. Check order status (must be Pending)
        if (order.Status != Domain.Enum.OrderStatus.Pending)
   {
            return BadRequest(new { Message = $"Order {request.OrderId} cannot be paid. Status: {order.Status}" });
        }
        
        // 3. Get client IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        
        // 4. Generate payment URL
   var paymentUrl = await _vnpayService.CreatePaymentUrlAsync(
            orderId: order.Id,
         amount: order.TotalAmount,
     orderInfo: $"Thanh toan don hang {order.OrderNumber}",
   ipAddress: ipAddress,
            cancellationToken: cancellationToken);
      
        _logger.LogInformation(
   "Created VNPay payment URL for Order {OrderId} by User {UserId}",
      order.Id, order.UserId);
     
 return Ok(new CreatePaymentResponse
        {
    OrderId = order.Id,
            PaymentUrl = paymentUrl,
ExpiresIn = 900 // 15 minutes
        });
    }
    
    /// <summary>
    /// VNPay Return URL - Handle user redirect after payment
  /// </summary>
    [HttpGet("vnpay/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayCallback(CancellationToken cancellationToken)
    {
   // 1. Extract query parameters
        var queryParams = Request.Query
      .ToDictionary(k => k.Key, v => v.Value.ToString());
    
        // 2. Process callback
        var result = await _vnpayService.ProcessReturnCallbackAsync(
            queryParams,
       cancellationToken);
      
        // 3. Redirect to frontend with result
 var frontendUrl = result.Success
   ? $"https://yourfrontend.com/payment/success?orderId={result.OrderId}&transactionId={result.TransactionId}"
            : $"https://yourfrontend.com/payment/failed?message={result.Message}";
        
  return Redirect(frontendUrl);
    }
    
    /// <summary>
    /// VNPay IPN (Webhook) - Server-to-server notification
    /// THIS IS THE MOST IMPORTANT ENDPOINT
    /// </summary>
    [HttpGet("vnpay/ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayIpn(CancellationToken cancellationToken)
    {
        // 1. Extract query parameters
   var queryParams = Request.Query
            .ToDictionary(k => k.Key, v => v.Value.ToString());

        _logger.LogInformation(
            "VNPay IPN received: {Params}",
     System.Text.Json.JsonSerializer.Serialize(queryParams));
        
        // 2. Process IPN
        var response = await _vnpayService.ProcessIpnAsync(
        queryParams,
     cancellationToken);
        
 // 3. Return response to VNPay
        return Ok(new
   {
 RspCode = response.RspCode,
            Message = response.Message
   });
    }
}

/// <summary>
/// Create payment request
/// </summary>
public record CreatePaymentRequest(Guid OrderId);

/// <summary>
/// Create payment response
/// </summary>
public record CreatePaymentResponse
{
    public Guid OrderId { get; init; }
    public string PaymentUrl { get; init; } = default!;
    public int ExpiresIn { get; init; }
}
