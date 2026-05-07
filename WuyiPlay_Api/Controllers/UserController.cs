using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;
 
        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
 
        /// <summary>Lấy danh sách tất cả user (Admin only)</summary>
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetAllUsers(pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Lấy thông tin user theo ID</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetUserById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
 
        /// <summary>Lấy thông tin user theo username</summary>
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var result = await _userService.GetUserByUsername(username);
            return result.Success ? Ok(result) : NotFound(result);
        }
 
        /// <summary>Cập nhật thông tin user</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.UpdateUser(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Đổi mật khẩu</summary>
        [HttpPut("{id:int}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.ChangePassword(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Xóa user (soft delete)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteUser(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Cập nhật số dư ví (Admin only)</summary>
        [HttpPut("{id:int}/balance")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> UpdateBalance(int id, [FromBody] UpdateBalanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.UpdateBalance(id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
