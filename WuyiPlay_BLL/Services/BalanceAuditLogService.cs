using AutoMapper;
using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class BalanceAuditLogService
{
    private readonly BalanceAuditLogRepository _repository;
    private readonly UserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BalanceAuditLogService> _logger;

    public BalanceAuditLogService(BalanceAuditLogRepository repository, UserRepository userRepository,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<BalanceAuditLogService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<PaginatedDto<BalanceAuditLogBasicDto>>> GetLogsByUser(
        int userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (userId <= 0) return Fail<PaginatedDto<BalanceAuditLogBasicDto>>("Invalid user ID");
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var logs = (await _repository.FindBy(l => l.UId == userId))
                .OrderByDescending(l => l.CreatedAt).ToList();

            var paged = logs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(_mapper.Map<BalanceAuditLogBasicDto>)
                .ToList();

            return Ok("Logs retrieved successfully",
                new PaginatedDto<BalanceAuditLogBasicDto>(paged, pageNumber, pageSize, logs.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs: {UserId}", userId);
            return Fail<PaginatedDto<BalanceAuditLogBasicDto>>("An error occurred while retrieving logs");
        }
    }

    public async Task<ApiResponseDto<BalanceAuditLogBasicDto>> TopUp(
        int userId, decimal amount, string reason = "Nạp tiền")
    {
        try
        {
            if (userId <= 0) return Fail<BalanceAuditLogBasicDto>("Invalid user ID");
            if (amount <= 0) return Fail<BalanceAuditLogBasicDto>("Amount must be greater than 0");

            var user = await _userRepository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1)
                return Fail<BalanceAuditLogBasicDto>("User not found");

            var before = user.Balance;
            user.Balance += amount;
            user.UpdatedAt = DateTime.Now;
            await _userRepository.Update(user);

            var log = await _repository.Create(new BalanceAuditLog
            {
                UId = userId,
                ChangeAmount = amount,
                BalanceBefore = before,
                BalanceAfter = user.Balance,
                Reason = reason,
                CreatedAt = DateTime.Now
            });

            _logger.LogInformation("TopUp OK: User {UserId}, Amount {Amount}", userId, amount);
            return Ok($"Top up successful. New balance: {user.Balance:N0}",
                _mapper.Map<BalanceAuditLogBasicDto>(log));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error topping up: {UserId}", userId);
            return Fail<BalanceAuditLogBasicDto>("An error occurred while topping up balance");
        }
    }

    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto<T> Fail<T>(string msg) => new(false, msg);
}
