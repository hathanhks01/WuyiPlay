using Microsoft.AspNetCore.Mvc;
using WuyiPlay_BLL.Services;
using WuyiPlay_DAL.DTOS;

namespace WuyiPlay_Api.Controllers
{
    /// <summary>
    /// Authentication Controller - Xử lý đăng nhập và đăng ký người dùng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        /// <param name="loginDto">Thông tin đăng nhập (Username và Password)</param>
        /// <returns>JWT Token nếu thành công</returns>
        /// <response code="200">Đăng nhập thành công, trả về token</response>
        /// <response code="400">Thông tin đăng nhập không hợp lệ</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validate request
                if (loginDto == null)
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Request body is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(loginDto.Username))
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Username is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Password is required"
                    });
                }

                // Gọi service login
                var result = await _authService.Login(loginDto);

                if (!result.Success)
                {
                    _logger.LogWarning($"Failed login attempt for username: {loginDto.Username}");
                    return Unauthorized(result);
                }

                _logger.LogInformation($"User {loginDto.Username} logged in successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login");
                return StatusCode(500, new AuthDto
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="registerDto">Thông tin đăng ký (Username và Password)</param>
        /// <returns>JWT Token nếu đăng ký thành công</returns>
        /// <response code="201">Đăng ký thành công</response>
        /// <response code="400">Thông tin đăng ký không hợp lệ hoặc tài khoản đã tồn tại</response>
        /// <response code="500">Lỗi server</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // Validate request
                if (registerDto == null)
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Request body is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(registerDto.Username))
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Username is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(registerDto.Password))
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Password is required"
                    });
                }

                // Validate password length (6-100 characters)
                if (registerDto.Password.Length < 6 || registerDto.Password.Length > 100)
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Password must be between 6 and 100 characters"
                    });
                }

                // Validate username length (3-50 characters)
                if (registerDto.Username.Length < 3 || registerDto.Username.Length > 50)
                {
                    return BadRequest(new AuthDto
                    {
                        Success = false,
                        Message = "Username must be between 3 and 50 characters"
                    });
                }

                // Gọi service register
                var result = await _authService.Register(registerDto);

                if (!result.Success)
                {
                    _logger.LogWarning($"Failed registration attempt for username: {registerDto.Username}. Reason: {result.Message}");
                    return BadRequest(result);
                }

                _logger.LogInformation($"New user registered: {registerDto.Username}");
                return CreatedAtAction(nameof(Register), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration");
                return StatusCode(500, new AuthDto
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Test endpoint - Kiểm tra API có chạy bình thường không
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object> Health()
        {
            return Ok(new
            {
                status = "OK",
                timestamp = DateTime.UtcNow,
                message = "WuyiPlay API is running"
            });
        }
    }
}
