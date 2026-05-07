using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WuyiPlay_DAL.Common;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(UserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthDto> Login(LoginDto loginDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                return Fail("Username and password are required");

            var user = await _userRepository.FirstOrDefault(u => u.Username == loginDto.Username);

            // Timing-attack prevention: luôn verify dù user null
            var hashToCheck = user?.HashPassword ?? BCrypt.Net.BCrypt.HashPassword("DUMMY");
            bool isPasswordValid = PasswordHelper.VerifyPassword(loginDto.Password, hashToCheck);

            if (user == null || user.IsDeleted == 1 || !isPasswordValid)
                return Fail("Invalid username or password");

            return new AuthDto
            {
                Success = true,
                Message = "Login successful",
                Token = GenerateJwtToken(user),
                Username = user.Username,
                UserId = user.UId,
                role = user.Role
            };
        }
        catch
        {
            return Fail("An error occurred during login");
        }
    }

    public async Task<AuthDto> Register(RegisterDto registerDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(registerDto.Username) || string.IsNullOrWhiteSpace(registerDto.Password))
                return Fail("Username and password are required");

            var exists = await _userRepository.Exists(u => u.Username == registerDto.Username);
            if (exists)
                return Fail("Username already exists");

            var newUser = new User
            {
                Username = registerDto.Username,
                HashPassword = PasswordHelper.HashPassword(registerDto.Password),
                Email = registerDto.Email,
                Phone = registerDto.PhoneNumber,
                Balance = 0,
                Role = 2, 
                IsDeleted = 0,
                CreatedAt = DateTime.Now
            };

            var created = await _userRepository.Create(newUser);

            return new AuthDto
            {
                Success = true,
                Message = "Registration successful",
                Token = GenerateJwtToken(created),
                Username = created.Username,
                UserId = created.UId,
                role = created.Role
            };
        }
        catch (Exception ex)
        {
            return Fail($"An error occurred: {ex.Message}");
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            // FIX: Dùng ClaimTypes.Role thay vì "role" string
            // "role" sẽ được tự động remap sang ClaimTypes.Role khi decode
            // → policy RequireClaim(ClaimTypes.Role, ...) sẽ match đúng
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthDto Fail(string message) => new() { Success = false, Message = message };
}
