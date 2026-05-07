using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
 
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }
 
        /// <summary>Lấy tất cả đơn hàng (Admin only)</summary>
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllOrders(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Lấy đơn hàng của một user</summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _orderService.GetOrdersByUser(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Lấy chi tiết đơn hàng theo ID</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _orderService.GetOrderById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
 
        /// <summary>Thanh toán toàn bộ giỏ hàng (Checkout)</summary>
        [HttpPost("checkout/{userId:int}")]
        public async Task<IActionResult> Checkout(int userId)
        {
            var result = await _orderService.CheckoutCart(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
