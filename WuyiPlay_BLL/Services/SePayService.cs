using Azure.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WuyiPlay_BLL.IServices;

namespace WuyiPlay_BLL.Services
{
    public class SePayService : ISePayService
    {
        private readonly IConfiguration _configuration;
        private readonly string _merchantId;
        private readonly string _secretKey;
        private readonly string _env;

        public SePayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GetCheckoutUrl()
        {
            return _env == _configuration["Sepay:env"]
                ? "https://sandbox.sepay.vn/payment"
                : "https://sepay.vn/payment";
        }
        public Dictionary<string, string> CreatePaymentFields(SePayPaymentRequest request)
        {
            var fields = new Dictionary<string, string>
        {
            { "merchant_id", _configuration["SePay:MerchantId"] },
            { "payment_method", "BANK_TRANSFER" },
            { "order_invoice_number", request.OrderInvoiceNumber },
            { "order_amount", request.OrderAmount.ToString("0") },
            { "currency", "VND" },
            { "order_description", request.OrderDescription },
            { "success_url", $"https://localhost:3000/payment/success" },
            { "error_url", $"https://localhost:3000/payment/error" },
            { "cancel_url", $"https://localhost:3000/payment/cancel" },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };

            // 🔥 Tạo chữ ký (signature)
            var signature = GenerateSignature(fields);
            fields.Add("signature", signature);

            return fields;
        }
        private string GenerateSignature(Dictionary<string, string> fields)
        {
            // sort theo key (giống Node SDK)
            var sorted = fields.OrderBy(x => x.Key);

            var rawData = string.Join("&", sorted.Select(x => $"{x.Key}={x.Value}"));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

    }
    public class SePayPaymentRequest
    {
        public string OrderInvoiceNumber { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
    }
}
