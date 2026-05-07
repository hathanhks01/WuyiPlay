using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
 
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }
 
        /// <summary>Lấy giỏ hàng của user</summary>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var result = await _cartService.GetCartByUser(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Thêm sản phẩm vào giỏ hàng</summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _cartService.AddToCart(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Xóa sản phẩm khỏi giỏ hàng</summary>
        [HttpDelete("{userId:int}/product/{productId:int}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId)
        {
            var result = await _cartService.RemoveFromCart(userId, productId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Xóa toàn bộ giỏ hàng</summary>
        [HttpDelete("{userId:int}/clear")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            var result = await _cartService.ClearCart(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
