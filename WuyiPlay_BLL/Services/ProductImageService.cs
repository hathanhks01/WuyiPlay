using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class ProductImageService
{
    private readonly ProductImageRepository _repository;
    private readonly ProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductImageService> _logger;
    private readonly string _uploadPath;
    private readonly string _publicPath;

    public ProductImageService(ProductImageRepository repository, ProductRepository productRepository,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductImageService> logger, IConfiguration config)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _uploadPath = config["FileStorage:UploadPath"] ?? "uploads";
        _publicPath = config["FileStorage:PublicPath"] ?? "/uploads";
    }

    public async Task<ApiResponseDto<List<ProductImageDto>>> GetImagesByProduct(int productId)
    {
        try
        {
            if (productId <= 0) return Fail<List<ProductImageDto>>("Invalid product ID");

            var images = await _repository.FindBy(i => i.PId == productId);
            return Ok("Images retrieved successfully",
                images.OrderBy(i => i.SortOrder)
                      .Select(_mapper.Map<ProductImageDto>).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting images: {PId}", productId);
            return Fail<List<ProductImageDto>>("An error occurred while retrieving images");
        }
    }

    public async Task<ApiResponseDto<ProductImageDto>> UploadImage(UploadProductImageDto dto)
    {
        try
        {
            if (dto?.ImageData == null || dto.ImageData.Length == 0)
                return Fail<ProductImageDto>("Image data is required");
            if (dto.PId <= 0) return Fail<ProductImageDto>("Invalid product ID");

            var product = await _productRepository.FirstOrDefault(p => p.PId == dto.PId);
            if (product == null) return Fail<ProductImageDto>("Product not found");

            var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!string.IsNullOrEmpty(dto.ContentType) && !allowed.Contains(dto.ContentType.ToLower()))
                return Fail<ProductImageDto>("Invalid image type. Allowed: jpg, png, gif, webp");

            if (dto.IsThumbnail == 1)
                await ClearThumbnails(dto.PId);

            var imageUrl = await SaveFileToDisk(dto);
            var created = await _repository.Create(new ProductImage
            {
                PId = dto.PId,
                ImageUrl = imageUrl,
                IsThumbnail = dto.IsThumbnail,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.Now
            });

            _logger.LogInformation("Image uploaded: {PId} → {Url}", dto.PId, imageUrl);
            return Ok("Image uploaded successfully", _mapper.Map<ProductImageDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {PId}", dto?.PId);
            return Fail<ProductImageDto>("An error occurred while uploading image");
        }
    }

    public async Task<ApiResponseDto<ProductImageDto>> AddImageUrl(CreateProductImageDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageUrl))
                return Fail<ProductImageDto>("Image URL is required");
            if (dto.PId <= 0) return Fail<ProductImageDto>("Invalid product ID");

            var product = await _productRepository.FirstOrDefault(p => p.PId == dto.PId);
            if (product == null) return Fail<ProductImageDto>("Product not found");

            if (dto.IsThumbnail == 1)
                await ClearThumbnails(dto.PId);

            var created = await _repository.Create(new ProductImage
            {
                PId = dto.PId,
                ImageUrl = dto.ImageUrl.Trim(),
                IsThumbnail = dto.IsThumbnail,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.Now
            });

            return Ok("Image added successfully", _mapper.Map<ProductImageDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image URL: {PId}", dto?.PId);
            return Fail<ProductImageDto>("An error occurred while adding image");
        }
    }

    public async Task<ApiResponseDto> DeleteImage(int imageId)
    {
        try
        {
            if (imageId <= 0) return Fail("Invalid image ID");

            var image = await _repository.FirstOrDefault(i => i.ImgId == imageId);
            if (image == null) return Fail("Image not found");

            // Xóa file vật lý nếu là file local
            if (image.ImageUrl.StartsWith(_publicPath))
            {
                var rel = image.ImageUrl.Substring(_publicPath.Length).TrimStart('/');
                var filePath = Path.Combine(_uploadPath, rel.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            await _repository.Delete(image);
            _logger.LogInformation("Image deleted: {ImgId}", imageId);
            return Ok("Image deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImgId}", imageId);
            return Fail("An error occurred while deleting image");
        }
    }

    private async Task ClearThumbnails(int productId)
    {
        var old = await _repository.FindBy(i => i.PId == productId && i.IsThumbnail == 1);
        foreach (var img in old)
        {
            img.IsThumbnail = 0;
            await _repository.UpdateNoSave(img);
        }
        await _unitOfWork.Commit();
    }

    private async Task<string> SaveFileToDisk(UploadProductImageDto dto)
    {
        var ext = Path.GetExtension(dto.FileName ?? ".jpg");
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var fileName = $"{Guid.NewGuid()}{ext}";
        var folder = Path.Combine(_uploadPath, "images", "products");
        Directory.CreateDirectory(folder);

        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), dto.ImageData);
        return $"{_publicPath}/images/products/{fileName}";
    }

    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto Ok(string msg) => new(true, msg);
    private static ApiResponseDto<T> Fail<T>(string msg) => new(false, msg);
    private static ApiResponseDto Fail(string msg) => new(false, msg);
}
