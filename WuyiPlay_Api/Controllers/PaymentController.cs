using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.IServices;
using WuyiPlay_BLL.Services;

namespace WuyiPlay_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ISePayService _sePayService;

        public PaymentController(ISePayService sePayService)
        {
            _sePayService = sePayService;
        }

        [HttpPost("create")]
        public IActionResult CreatePayment([FromBody] SePayPaymentRequest request)
        {
            var fields = _sePayService.CreatePaymentFields(request);
            var checkoutUrl = _sePayService.GetCheckoutUrl();

            return Ok(new
            {
                url = checkoutUrl,
                fields = fields
            });
        }
        [HttpGet("payment/access")]
        public async Task<IActionResult> access()
        {
            return await Task.FromResult<IActionResult>(Ok(new { Message = "Payment is working!" }));
        }
        [HttpGet("payment/error")]
        public async Task<IActionResult> error()
        {
            return await Task.FromResult<IActionResult>(BadRequest(new { Message = "Payment is error!" }));
        }
        [HttpGet("payment/cancel")]
        public async Task<IActionResult> cancel()
        {
            return await Task.FromResult<IActionResult>(Ok(new { Message = "Payment is cancel!" }));
        }
    }
}
