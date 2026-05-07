using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
 
namespace WuyiPlay_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BalanceAuditLogController : ControllerBase
    {
        private readonly BalanceAuditLogService _auditService;
 
        public BalanceAuditLogController(BalanceAuditLogService auditService)
        {
            _auditService = auditService;
        }
 
        /// <summary>Lấy lịch sử giao dịch của user</summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _auditService.GetLogsByUser(userId, pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
 
        /// <summary>Nạp tiền cho user (Admin only)</summary>
        [HttpPost("topup/{userId:int}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> TopUp(int userId,
            [FromQuery] decimal amount, [FromQuery] string reason = "Nạp tiền")
        {
            var result = await _auditService.TopUp(userId, amount, reason);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
