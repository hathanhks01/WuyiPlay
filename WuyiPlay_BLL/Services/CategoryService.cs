using AutoMapper;
using Microsoft.Extensions.Logging;
using WuyiPlay_DAL.Common.Repository;
using WuyiPlay_DAL.DTOS;
using WuyiPlay_DAL.Models;
using WuyiPlay_DAL.Repositories;

namespace WuyiPlay_BLL.Services;

public class CategoryService
{
    private readonly CategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(CategoryRepository repository, IUnitOfWork unitOfWork,
        IMapper mapper, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponseDto<List<CategoryBasicDto>>> GetAllCategories()
    {
        try
        {
            var list = await _repository.GetAll();
            return Ok("Categories retrieved successfully",
                list.Select(_mapper.Map<CategoryBasicDto>).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return Fail<List<CategoryBasicDto>>("An error occurred while retrieving categories");
        }
    }

    public async Task<ApiResponseDto<CategoryDto>> GetCategoryById(int categoryId)
    {
        try
        {
            if (categoryId <= 0) return Fail<CategoryDto>("Invalid category ID");

            var category = await _repository.FirstOrDefault(
                c => c.CId == categoryId, c => c.Products);
            if (category == null) return Fail<CategoryDto>("Category not found");

            return Ok("Category retrieved successfully", _mapper.Map<CategoryDto>(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category: {CategoryId}", categoryId);
            return Fail<CategoryDto>("An error occurred while retrieving category");
        }
    }

    public async Task<ApiResponseDto<CategoryBasicDto>> CreateCategory(CreateCategoryDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return Fail<CategoryBasicDto>("Category name is required");

            if (await _repository.Exists(c => c.Name == dto.Name.Trim()))
                return Fail<CategoryBasicDto>("Category name already exists");

            var created = await _repository.Create(new Category { Name = dto.Name.Trim() });
            _logger.LogInformation("Category created: {CId}", created.CId);
            return Ok("Category created successfully", _mapper.Map<CategoryBasicDto>(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Fail<CategoryBasicDto>("An error occurred while creating category");
        }
    }

    public async Task<ApiResponseDto<CategoryBasicDto>> UpdateCategory(int categoryId, UpdateCategoryDto dto)
    {
        try
        {
            if (categoryId <= 0) return Fail<CategoryBasicDto>("Invalid category ID");
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return Fail<CategoryBasicDto>("Category name is required");

            var category = await _repository.FirstOrDefault(c => c.CId == categoryId);
            if (category == null) return Fail<CategoryBasicDto>("Category not found");

            if (await _repository.Exists(c => c.Name == dto.Name.Trim() && c.CId != categoryId))
                return Fail<CategoryBasicDto>("Category name already exists");

            category.Name = dto.Name.Trim();
            await _repository.Update(category);

            _logger.LogInformation("Category updated: {CId}", categoryId);
            return Ok("Category updated successfully", _mapper.Map<CategoryBasicDto>(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category: {CId}", categoryId);
            return Fail<CategoryBasicDto>("An error occurred while updating category");
        }
    }

    public async Task<ApiResponseDto> DeleteCategory(int categoryId)
    {
        try
        {
            if (categoryId <= 0) return Fail("Invalid category ID");

            var category = await _repository.FirstOrDefault(c => c.CId == categoryId);
            if (category == null) return Fail("Category not found");

            await _repository.Delete(category);
            _logger.LogInformation("Category deleted: {CId}", categoryId);
            return Ok("Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category: {CId}", categoryId);
            return Fail("An error occurred while deleting category");
        }
    }

    private static ApiResponseDto<T> Ok<T>(string msg, T data) => new(true, msg, data);
    private static ApiResponseDto Ok(string msg) => new(true, msg);
    private static ApiResponseDto<T> Fail<T>(string msg) => new(false, msg);
    private static ApiResponseDto Fail(string msg) => new(false, msg);
}
