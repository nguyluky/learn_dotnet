using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using Mapster;

namespace test_api.Services
{
    public interface ICategoryService
    {
        Task<PagedResultDto<CategoryDto>> GetCategoriesAsync(PaginationDto pagination);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly StoreContext _context;

        public CategoryService(StoreContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<CategoryDto>> GetCategoriesAsync(PaginationDto pagination)
        {
            var query = _context.Categories.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(c => c.Name.Contains(pagination.Search) || 
                                       c.Description.Contains(pagination.Search));
            }

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "name_asc" => query.OrderBy(c => c.Name),
                    "name_desc" => query.OrderByDescending(c => c.Name),
                    "date_asc" => query.OrderBy(c => c.CreatedAt),
                    "date_desc" => query.OrderByDescending(c => c.CreatedAt),
                    _ => query.OrderByDescending(c => c.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var categories = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsEnabled = c.IsEnabled,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.Products.Count()
                })
                .ToListAsync();

            return new PagedResultDto<CategoryDto>
            {
                Data = categories,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsEnabled = c.IsEnabled,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ProductCount = c.Products.Count()
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            return category;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = createCategoryDto.Adapt<Category>();
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return await GetCategoryByIdAsync(category.Id) ?? throw new InvalidOperationException("Failed to create category");
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;

            updateCategoryDto.Adapt(category);
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetCategoryByIdAsync(id);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            // Check if category has products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
                return false; // Cannot delete category with products

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
