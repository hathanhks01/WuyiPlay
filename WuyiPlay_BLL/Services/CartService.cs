using AutoMapper;
using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class CartService
{
    private readonly CartRepository _repository;
    private readonly ProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CartService> _logger;

    public CartService(CartRepository repository, ProductRepository productRepository,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<CartSummaryDto>> GetCartByUser(int userId)
    {
        try
        {
            if (userId <= 0) return Fail<CartSummaryDto>("Invalid user ID");

            var items = (await _repository.FindBy(
                c => c.UId == userId, c => c.PIdNavigation)).ToList();

            var cartDtos = items.Select(_mapper.Map<CartDto>).ToList();

            return Ok("Cart retrieved successfully", new CartSummaryDto
            {
                UId = userId,
                TotalItems = cartDtos.Count,
                TotalPrice = cartDtos.Sum(i => i.Product?.Price ?? 0),
                Items = cartDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart: {UserId}", userId);
            return Fail<CartSummaryDto>("An error occurred while retrieving cart");
        }
    }

    public async Task<ApiResponseDto<CartBasicDto>> AddToCart(AddToCartDto dto)
    {
        try
        {
            if (dto == null) return Fail<CartBasicDto>("Cart data is required");
            if (dto.UId <= 0) return Fail<CartBasicDto>("Invalid user ID");
            if (dto.PId <= 0) return Fail<CartBasicDto>("Invalid product ID");

            var product = await _productRepository.FirstOrDefault(p => p.PId == dto.PId);
            if (product == null) return Fail<CartBasicDto>("Product not found");
            if (product.Status == 0) return Fail<CartBasicDto>("Product is no longer available");

            if (await _repository.Exists(c => c.UId == dto.UId && c.PId == dto.PId))
                return Fail<CartBasicDto>("Product already in cart");

            if (await _repository.Exists(c => c.PId == dto.PId && c.UId != dto.UId))
                return Fail<CartBasicDto>("Product is already reserved by another user");

            var created = await _repository.Create(new Cart
            {
                UId = dto.UId,
                PId = dto.PId,
                AddedAt = DateTime.Now
            });

            _logger.LogInformation("Added to cart: User {UId}, Product {PId}", dto.UId, dto.PId);
            return Ok("Product added to cart successfully", _mapper.Map<CartBasicDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart");
            return Fail<CartBasicDto>("An error occurred while adding to cart");
        }
    }

    public async Task<ApiResponseDto> RemoveFromCart(int userId, int productId)
    {
        try
        {
            if (userId <= 0 || productId <= 0) return Fail("Invalid user ID or product ID");

            var item = await _repository.FirstOrDefault(c => c.UId == userId && c.PId == productId);
            if (item == null) return Fail("Cart item not found");

            await _repository.Delete(item);
            _logger.LogInformation("Removed from cart: User {UId}, Product {PId}", userId, productId);
            return Ok("Product removed from cart successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cart");
            return Fail("An error occurred while removing from cart");
        }
    }

    public async Task<ApiResponseDto> ClearCart(int userId)
    {
        try
        {
            if (userId <= 0) return Fail("Invalid user ID");

            await _repository.DeleteRange(c => c.UId == userId);
            _logger.LogInformation("Cart cleared: {UserId}", userId);
            return Ok("Cart cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart: {UserId}", userId);
            return Fail("An error occurred while clearing cart");
        }
    }

    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto Ok(string msg) => new(true, msg);
    private static ApiResponseDto<T> Fail<T>(string msg) => new(false, msg);
    private static ApiResponseDto Fail(string msg) => new(false, msg);
}
