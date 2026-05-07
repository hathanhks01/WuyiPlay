using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
 
        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }
 
        /// <summary>Lấy tất cả danh mục</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategories();
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Lấy danh mục theo ID (kèm danh sách sản phẩm)</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetCategoryById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
 
        /// <summary>Tạo danh mục mới (Admin only)</summary>
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _categoryService.CreateCategory(dto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.CId }, result) : BadRequest(result);
        }
 
        /// <summary>Cập nhật danh mục (Admin only)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _categoryService.UpdateCategory(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Xóa danh mục (Admin only)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategory(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
