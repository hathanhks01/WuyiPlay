using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using WuyiPlay_DAL.Common;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class UserService
{
    private readonly UserRepository _repository;
    private readonly BalanceAuditLogRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(UserRepository repository, BalanceAuditLogRepository auditRepository,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _repository = repository;
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PaginatedDto<UserDto>>> GetAllUsers(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var users = await _repository.GetAll();
            var active = users.Where(u => u.IsDeleted == 0).ToList();

            var paged = active
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(_mapper.Map<UserDto>)
                .ToList();

            return Ok("Users retrieved successfully",
                new PaginatedDto<UserDto>(paged, pageNumber, pageSize, active.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return Fail<PaginatedDto<UserDto>>("An error occurred while retrieving users");
        }
    }

    public async Task<ApiResponseDto<UserDto>> GetUserById(int userId)
    {
        try
        {
            if (userId <= 0) return Fail<UserDto>("Invalid user ID");

            var user = await _repository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1) return Fail<UserDto>("User not found");

            return Ok("User retrieved successfully", _mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return Fail<UserDto>("An error occurred while retrieving user");
        }
    }

    public async Task<ApiResponseDto<UserDto>> GetUserByUsername(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username)) return Fail<UserDto>("Username is required");

            var user = await _repository.FirstOrDefault(u => u.Username == username);
            if (user == null || user.IsDeleted == 1) return Fail<UserDto>("User not found");

            return Ok("User retrieved successfully", _mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return Fail<UserDto>("An error occurred while retrieving user");
        }
    }

    public async Task<ApiResponseDto<UserDto>> UpdateUser(int userId, UpdateUserDto dto)
    {
        try
        {
            if (userId <= 0) return Fail<UserDto>("Invalid user ID");
            if (dto == null) return Fail<UserDto>("User data is required");

            var user = await _repository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1) return Fail<UserDto>("User not found");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (!IsValidEmail(dto.Email)) return Fail<UserDto>("Invalid email format");
                var dup = await _repository.FirstOrDefault(u => u.Email == dto.Email && u.UId != userId);
                if (dup != null) return Fail<UserDto>("Email already exists");
                user.Email = dto.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                if (!IsValidPhone(dto.Phone)) return Fail<UserDto>("Invalid phone format");
                var dup = await _repository.FirstOrDefault(u => u.Phone == dto.Phone && u.UId != userId);
                if (dup != null) return Fail<UserDto>("Phone already exists");
                user.Phone = dto.Phone.Trim();
            }

            if (dto.Role != user.Role) user.Role = dto.Role;
            user.UpdatedAt = DateTime.Now;

            await _repository.Update(user);
            _logger.LogInformation("User updated: {UserId}", userId);
            return Ok("User updated successfully", _mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", userId);
            return Fail<UserDto>("An error occurred while updating user");
        }
    }

    public async Task<ApiResponseDto> ChangePassword(int userId, ChangePasswordDto dto)
    {
        try
        {
            if (userId <= 0) return Fail("Invalid user ID");
            if (dto == null) return Fail("Password data is required");
            if (string.IsNullOrWhiteSpace(dto.OldPassword)) return Fail("Old password is required");
            if (string.IsNullOrWhiteSpace(dto.NewPassword)) return Fail("New password is required");
            if (dto.NewPassword.Length < 6 || dto.NewPassword.Length > 100)
                return Fail("New password must be between 6 and 100 characters");

            var user = await _repository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1) return Fail("User not found");
            if (!PasswordHelper.VerifyPassword(dto.OldPassword, user.HashPassword))
                return Fail("Old password is incorrect");

            user.HashPassword = PasswordHelper.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _repository.Update(user);

            _logger.LogInformation("Password changed: {UserId}", userId);
            return Ok("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password: {UserId}", userId);
            return Fail("An error occurred while changing password");
        }
    }

    public async Task<ApiResponseDto> DeleteUser(int userId)
    {
        try
        {
            if (userId <= 0) return Fail("Invalid user ID");

            var user = await _repository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1) return Fail("User not found");

            user.IsDeleted = 1;
            user.UpdatedAt = DateTime.Now;
            await _repository.Update(user);

            _logger.LogInformation("User soft-deleted: {UserId}", userId);
            return Ok("User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return Fail("An error occurred while deleting user");
        }
    }

    public async Task<ApiResponseDto<UserDto>> UpdateBalance(int userId, UpdateBalanceDto dto)
    {
        try
        {
            if (userId <= 0) return Fail<UserDto>("Invalid user ID");
            if (dto == null) return Fail<UserDto>("Balance data is required");
            if (dto.Amount == 0) return Fail<UserDto>("Amount cannot be zero");

            var user = await _repository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1) return Fail<UserDto>("User not found");

            var before = user.Balance;
            user.Balance += dto.Amount;
            if (user.Balance < 0) return Fail<UserDto>($"Insufficient balance. Available: {before}");

            user.UpdatedAt = DateTime.Now;
            await _repository.Update(user);

            // Ghi audit log
            await _auditRepository.Create(new BalanceAuditLog
            {
                UId = userId,
                ChangeAmount = dto.Amount,
                BalanceBefore = before,
                BalanceAfter = user.Balance,
                Reason = string.IsNullOrWhiteSpace(dto.Reason) ? "Admin cập nhật số dư" : dto.Reason,
                RefId = null, // Admin nạp thủ công không có mã giao dịch
                CreatedAt = DateTime.Now
            });

            _logger.LogInformation("Balance updated: {UserId} {Before} → {After}", userId, before, user.Balance);
            return Ok("Balance updated successfully", _mapper.Map<UserDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance: {UserId}", userId);
            return Fail<UserDto>("An error occurred while updating balance");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email; }
        catch { return false; }
    }

    private static bool IsValidPhone(string phone) =>
        Regex.IsMatch(phone, @"^\d{10,15}$");

    private static ApiResponseDto<T> Ok<T>(string msg, T data) =>
        new(true, msg, data);

    private static ApiResponseDto Ok(string msg) =>
        new(true, msg);

    private static ApiResponseDto<T> Fail<T>(string msg) =>
        new(false, msg);

    private static ApiResponseDto Fail(string msg) =>
        new(false, msg);
}
