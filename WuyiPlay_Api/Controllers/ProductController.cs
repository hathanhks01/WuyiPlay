using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;
 
        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }
 
        /// <summary>Lấy danh sách tất cả sản phẩm (có filter + phân trang)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? status = null)
        {
            var result = await _productService.GetAllProducts(pageNumber, pageSize, categoryId, status);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Lấy chi tiết sản phẩm theo ID (kèm ảnh và danh mục)</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
 
        /// <summary>Lấy danh sách sản phẩm còn hàng (Status = 1)</summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int? categoryId = null)
        {
            var result = await _productService.GetAvailableProducts(categoryId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Tạo sản phẩm mới (Admin/Collaborator)</summary>
        [HttpPost]
        [Authorize(Policy = "RequireAdminOrCollaborator")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _productService.CreateProduct(dto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.PId }, result) : BadRequest(result);
        }
 
        /// <summary>Cập nhật sản phẩm (Admin/Collaborator)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "RequireAdminOrCollaborator")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _productService.UpdateProduct(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Xóa sản phẩm (Admin only)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProduct(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
