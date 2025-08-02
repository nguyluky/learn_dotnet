using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using Mapster;
using test_api.Controllers;

namespace test_api.Services
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductDto>> GetProductsAsync(GetProductsResquest pagination);
        Task<PagedResultDto<ProductDto>> GetProductsByStoreAsync(int storeId, PaginationDto pagination);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
    }

    public class ProductService : IProductService
    {
        private readonly StoreContext _context;

        public ProductService(StoreContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsAsync(GetProductsResquest pagination)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => p.Name.Contains(pagination.Search) || 
                                       p.Description.Contains(pagination.Search));
            }

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "name_asc" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "date_asc" => query.OrderBy(p => p.CreatedAt),
                    "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            // Filter by category
            if (pagination.Category > 0)
            {
                query = query.Where(p => p.CategoryId == pagination.Category.Value);
            }

            // Filter by price range
            if (pagination.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= pagination.MinPrice.Value);
            }

            if (pagination.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= pagination.MaxPrice.Value);
            }

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                ShortDescription = p.ShortDescription,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StoreId = p.StoreId,
                StoreName = p.Store.Name,
                IsAvailable = p.IsAvailable,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return new PagedResultDto<ProductDto>
            {
                Data = productDtos,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsByStoreAsync(int storeId, PaginationDto pagination)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Where(p => p.StoreId == storeId)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => p.Name.Contains(pagination.Search) || 
                                       p.Description.Contains(pagination.Search));
            }

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "name_asc" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "date_asc" => query.OrderBy(p => p.CreatedAt),
                    "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                ShortDescription = p.ShortDescription,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                StoreId = p.StoreId,
                StoreName = p.Store.Name,
                IsAvailable = p.IsAvailable,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return new PagedResultDto<ProductDto>
            {
                Data = productDtos,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                StoreId = product.StoreId,
                StoreName = product.Store.Name,
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = createProductDto.Adapt<Product>();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id) ?? throw new InvalidOperationException("Failed to create product");
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return null;

            updateProductDto.Adapt(product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetProductByIdAsync(id);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
