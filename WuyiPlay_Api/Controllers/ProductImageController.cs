using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImageController : ControllerBase
    {
        private readonly ProductImageService _imageService;
 
        public ProductImageController(ProductImageService imageService)
        {
            _imageService = imageService;
        }
 
        /// <summary>Lấy danh sách ảnh của sản phẩm</summary>
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _imageService.GetImagesByProduct(productId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Thêm ảnh qua URL (Admin/Collaborator)</summary>
        [HttpPost("url")]
        [Authorize(Policy = "RequireAdminOrCollaborator")]
        public async Task<IActionResult> AddByUrl([FromBody] CreateProductImageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _imageService.AddImageUrl(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Upload ảnh từ file (Admin/Collaborator)</summary>
        [HttpPost("upload")]
        [Authorize(Policy = "RequireAdminOrCollaborator")]
        public async Task<IActionResult> Upload([FromForm] UploadImageFormDto form)
        {
            if (form.File == null || form.File.Length == 0)
                return BadRequest(new { Success = false, Message = "File is required" });

            using var ms = new MemoryStream();
            await form.File.CopyToAsync(ms);

            var dto = new UploadProductImageDto
            {
                PId = form.ProductId,
                ImageData = ms.ToArray(),
                FileName = form.File.FileName,
                ContentType = form.File.ContentType,
                IsThumbnail = form.IsThumbnail,
                SortOrder = form.SortOrder
            };

            var result = await _imageService.UploadImage(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        public class UploadImageFormDto
        {
            public IFormFile File { get; set; }
            public int ProductId { get; set; }
            public int IsThumbnail { get; set; } = 0;
            public int SortOrder { get; set; } = 0;
        }
        /// <summary>Xóa ảnh (Admin/Collaborator)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdminOrCollaborator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _imageService.DeleteImage(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}


