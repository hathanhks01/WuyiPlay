using AutoMapper;
using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class ProductService
{
    private readonly ProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductRepository repository, IUnitOfWork unitOfWork,
        IMapper mapper, ILogger<ProductService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  READ
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedDto<ProductBasicDto>>> GetAllProducts(
        int pageNumber = 1, int pageSize = 10, int? categoryId = null, int? status = null)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // Dùng FindByPaged để phân trang tại DB (không load all vào RAM)
            var (items, total) = await _repository.FindByPaged(
                predicate: p => (categoryId == null || p.CId == categoryId)
                             && (status == null || p.Status == status),
                orderBy:   p => p.PId,
                pageSize:  pageSize,
                pageIndex: pageNumber,
                descending: true
            );

            // FindByPaged chưa hỗ trợ Include nên load navigations riêng khi cần
            // (dùng FindBy với includes cho detail; list chỉ cần ảnh thumbnail)
            var paged = items.Select(_mapper.Map<ProductBasicDto>).ToList();

            return Ok("Products retrieved successfully",
                new PaginatedDto<ProductBasicDto>(paged, pageNumber, pageSize, total));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return Fail<PaginatedDto<ProductBasicDto>>("An error occurred while retrieving products");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> GetProductById(int productId)
    {
        try
        {
            if (productId <= 0) return Fail<ProductDto>("Invalid product ID");

            var product = await _repository.FirstOrDefault(
                p => p.PId == productId,
                p => p.CIdNavigation,
                p => p.ProductImages);

            if (product == null) return Fail<ProductDto>("Product not found");

            return Ok("Product retrieved successfully", _mapper.Map<ProductDto>(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product: {PId}", productId);
            return Fail<ProductDto>("An error occurred while retrieving product");
        }
    }

    public async Task<ApiResponseDto<List<ProductBasicDto>>> GetAvailableProducts(int? categoryId = null)
    {
        try
        {
            var products = await _repository.FindBy(
                p => p.Status == 1 && (categoryId == null || p.CId == categoryId),
                p => p.CIdNavigation,
                p => p.ProductImages);

            return Ok("Available products retrieved successfully",
                products.Select(_mapper.Map<ProductBasicDto>).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available products");
            return Fail<List<ProductBasicDto>>("An error occurred while retrieving products");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  WRITE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<ProductBasicDto>> CreateProduct(CreateProductDto dto)
    {
        try
        {
            if (dto == null)                          return Fail<ProductBasicDto>("Product data is required");
            if (string.IsNullOrWhiteSpace(dto.UserName)) return Fail<ProductBasicDto>("Account username is required");
            if (dto.Price < 0 || dto.CostPrice < 0)   return Fail<ProductBasicDto>("Price cannot be negative");

            // ── Xác định PCode ─────────────────────────────────────────────
            // Nếu admin cung cấp mã tuỳ chỉnh thì dùng, ngược lại đặt PENDING
            // và tự sinh WP-XXXXXX sau khi có PId.
            var customCode = dto.PCode?.Trim();
            if (!string.IsNullOrWhiteSpace(customCode))
            {
                // Kiểm tra trùng
                var duplicate = await _repository.Exists(p => p.PCode == customCode);
                if (duplicate)
                    return Fail<ProductBasicDto>($"Mã sản phẩm '{customCode}' đã tồn tại.");
            }

            var product = new Product
            {
                CId       = dto.CId == 0 ? null : dto.CId,
                Username  = dto.UserName.Trim(),           // Tên đăng nhập game (bí mật)
                Password  = dto.Password ?? string.Empty,  // Mật khẩu game (bí mật)
                Describe  = dto.Description,
                Price     = dto.Price,
                CostPrice = dto.CostPrice,
                Status    = (int)DataType.ProductStatus.Available,
                CreatedAt = DateTime.Now,
                PCode     = string.IsNullOrWhiteSpace(customCode) ? "PENDING" : customCode
            };

            var created = await _repository.Create(product);

            // Tự sinh mã WP-XXXXXX nếu chưa có mã tuỳ chỉnh
            if (string.IsNullOrWhiteSpace(customCode))
            {
                created.PCode = $"WP-{created.PId:D6}";
                await _repository.Update(created);
            }

            _logger.LogInformation("Product created: PId={PId}, PCode={PCode}", created.PId, created.PCode);
            return Ok("Product created successfully", _mapper.Map<ProductBasicDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Fail<ProductBasicDto>("An error occurred while creating product");
        }
    }

    public async Task<ApiResponseDto<ProductBasicDto>> UpdateProduct(int productId, UpdateProductDto dto)
    {
        try
        {
            if (productId <= 0) return Fail<ProductBasicDto>("Invalid product ID");
            if (dto == null)    return Fail<ProductBasicDto>("Product data is required");

            var product = await _repository.FirstOrDefault(p => p.PId == productId);
            if (product == null)     return Fail<ProductBasicDto>("Product not found");
            if (product.Status == 0) return Fail<ProductBasicDto>("Cannot update a sold product");
            product.CId       = dto.CId == 0 ? null : dto.CId;
            if (!string.IsNullOrWhiteSpace(dto.Description)) product.Describe = dto.Description;
            product.Price     = dto.Price;
            product.CostPrice = dto.CostPrice;
            product.Status    = dto.Status;

            await _repository.Update(product);
            _logger.LogInformation("Product updated: PId={PId}, PCode={PCode}", productId, product.PCode);
            return Ok("Product updated successfully", _mapper.Map<ProductBasicDto>(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {PId}", productId);
            return Fail<ProductBasicDto>("An error occurred while updating product");
        }
    }

    public async Task<ApiResponseDto> DeleteProduct(int productId)
    {
        try
        {
            if (productId <= 0) return Fail("Invalid product ID");

            var product = await _repository.FirstOrDefault(p => p.PId == productId);
            if (product == null)     return Fail("Product not found");
            if (product.Status == 0) return Fail("Cannot delete a sold product");

            await _repository.Delete(product);
            _logger.LogInformation("Product deleted: PId={PId}", productId);
            return Ok("Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {PId}", productId);
            return Fail("An error occurred while deleting product");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────
    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto     Ok(string msg)            => new(true, msg);
    private static ApiResponseDto<T> Fail<T>(string msg)       => new(false, msg);
    private static ApiResponseDto     Fail(string msg)          => new(false, msg);
}
