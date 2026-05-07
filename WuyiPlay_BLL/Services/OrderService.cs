using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class OrderService
{
    private readonly OrderRepository _repository;
    private readonly ProductRepository _productRepository;
    private readonly UserRepository _userRepository;
    private readonly CartRepository _cartRepository;
    private readonly BalanceAuditLogRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrderRepository repository, ProductRepository productRepository,
        UserRepository userRepository, CartRepository cartRepository,
        BalanceAuditLogRepository auditRepository,
        IUnitOfWork unitOfWork, ILogger<OrderService> logger)
    {
        _repository = repository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _cartRepository = cartRepository;
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  READ
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedDto<OrderBasicDto>>> GetAllOrders(
        int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var all = (await _repository.FindBy(o => true, o => o.PIdNavigation))
                .OrderByDescending(o => o.CreatedAt).ToList();

            var paged = all
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToBasicDto)
                .ToList();

            return Ok("Orders retrieved successfully",
                new PaginatedDto<OrderBasicDto>(paged, pageNumber, pageSize, all.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return Fail<PaginatedDto<OrderBasicDto>>("An error occurred while retrieving orders");
        }
    }

    public async Task<ApiResponseDto<List<OrderBasicDto>>> GetOrdersByUser(int userId)
    {
        try
        {
            if (userId <= 0) return Fail<List<OrderBasicDto>>("Invalid user ID");

            var orders = await _repository.FindBy(o => o.UId == userId, o => o.PIdNavigation);
            return Ok("Orders retrieved successfully",
                orders.OrderByDescending(o => o.CreatedAt).Select(MapToBasicDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user: {UserId}", userId);
            return Fail<List<OrderBasicDto>>("An error occurred while retrieving orders");
        }
    }

    public async Task<ApiResponseDto<OrderDto>> GetOrderById(int orderId)
    {
        try
        {
            if (orderId <= 0) return Fail<OrderDto>("Invalid order ID");

            var order = await _repository.FirstOrDefault(
                o => o.OId == orderId,
                o => o.UIdNavigation,
                o => o.PIdNavigation);

            if (order == null) return Fail<OrderDto>("Order not found");
            return Ok("Order retrieved successfully", MapToDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order: {OId}", orderId);
            return Fail<OrderDto>("An error occurred while retrieving order");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CHECKOUT — có transaction để đảm bảo toàn vẹn dữ liệu
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<OrderBasicDto>>> CheckoutCart(int userId)
    {
        using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            if (userId <= 0) return Fail<List<OrderBasicDto>>("Invalid user ID");

            var user = await _userRepository.FirstOrDefault(u => u.UId == userId);
            if (user == null || user.IsDeleted == 1)
                return Fail<List<OrderBasicDto>>("User not found");

            var cartItems = (await _cartRepository.FindBy(
                c => c.UId == userId,
                c => c.PIdNavigation)).ToList();

            if (!cartItems.Any())
                return Fail<List<OrderBasicDto>>("Cart is empty");

            var available = cartItems.Where(c => c.PIdNavigation?.Status == 1).ToList();
            if (!available.Any())
                return Fail<List<OrderBasicDto>>("No available products in cart");

            var total = available.Sum(c => c.PIdNavigation!.Price);
            if (user.Balance < total)
                return Fail<List<OrderBasicDto>>(
                    $"Insufficient balance. Required: {total:N0}, Available: {user.Balance:N0}");

            // Trừ tiền
            var before = user.Balance;
            user.Balance -= total;
            user.UpdatedAt = DateTime.Now;
            await _userRepository.Update(user);

            // Ghi audit log
            await _auditRepository.Create(new BalanceAuditLog
            {
                UId           = userId,
                ChangeAmount  = -total,
                BalanceBefore = before,
                BalanceAfter  = user.Balance,
                Reason        = "Thanh toán giỏ hàng",
                CreatedAt     = DateTime.Now
            });

            // Tạo đơn hàng + đánh dấu sản phẩm đã bán
            var createdOrders = new List<OrderBasicDto>();
            foreach (var item in available)
            {
                var order = await _repository.Create(new Order
                {
                    UId           = userId,
                    PId           = item.PId,
                    Amount        = item.PIdNavigation!.Price,
                    PaymentMethod = "Ví WuyiPlay",
                    CreatedAt     = DateTime.Now
                });

                // Gán lại navigation để MapToBasicDto có đủ dữ liệu tài khoản
                order.PIdNavigation = item.PIdNavigation;
                createdOrders.Add(MapToBasicDto(order));

                item.PIdNavigation.Status = 0;
                item.PIdNavigation.SoldAt = DateTime.Now;
                await _productRepository.Update(item.PIdNavigation);
            }

            // Xóa giỏ hàng
            await _cartRepository.DeleteRange(c => c.UId == userId);

            await transaction.CommitAsync();
            _logger.LogInformation("Checkout OK: User {UserId}, Total {Total}", userId, total);
            return Ok($"Checkout successful. Total paid: {total:N0}", createdOrders);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Checkout FAILED (rolled back): User {UserId}", userId);
            return Fail<List<OrderBasicDto>>("An error occurred during checkout. Transaction rolled back.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MAPPING — dùng PCode làm ProductName (tên hiển thị an toàn)
    //            Username/Password chỉ trả về sau khi mua thành công
    // ─────────────────────────────────────────────────────────────────────────

    private static OrderBasicDto MapToBasicDto(Order o) => new()
    {
        OId           = o.OId,
        UId           = o.UId,
        PId           = o.PId,
        Amount        = o.Amount,
        PaymentMethod = o.PaymentMethod,
        CreatedAt     = o.CreatedAt,
        // Thông tin tài khoản game — chỉ trả về sau khi đã thanh toán thành công
        AccountUsername = o.PIdNavigation?.Username,
        AccountPassword = o.PIdNavigation?.Password
    };

    private static OrderDto MapToDto(Order o) => new()
    {
        OId           = o.OId,
        UId           = o.UId,
        PId           = o.PId,
        Amount        = o.Amount,
        PaymentMethod = o.PaymentMethod,
        CreatedAt     = o.CreatedAt,

        // Thông tin tài khoản game — endpoint này yêu cầu auth nên ok để trả về
        AccountUsername = o.PIdNavigation?.Username,
        AccountPassword = o.PIdNavigation?.Password,

        User = o.UIdNavigation == null ? null : new UserDto
        {
            UId       = o.UIdNavigation.UId,
            Username  = o.UIdNavigation.Username,
            Email     = o.UIdNavigation.Email,
            Phone     = o.UIdNavigation.Phone,
            Balance   = o.UIdNavigation.Balance,
            Role      = o.UIdNavigation.Role,
            IsDeleted = o.UIdNavigation.IsDeleted,
            CreatedAt = o.UIdNavigation.CreatedAt,
            UpdatedAt = o.UIdNavigation.UpdatedAt
        },

        Product = o.PIdNavigation == null ? null : new ProductBasicDto
        {
            PId         = o.PIdNavigation.PId,
            CId         = o.PIdNavigation.CId ?? 0,
            PCode       = o.PIdNavigation.PCode,
            Description = o.PIdNavigation.Describe,
            Price       = o.PIdNavigation.Price,
            CostPrice   = o.PIdNavigation.CostPrice,
            Status      = o.PIdNavigation.Status,
            CreatedAt   = o.PIdNavigation.CreatedAt
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto<T> Fail<T>(string msg)       => new(false, msg);
}
