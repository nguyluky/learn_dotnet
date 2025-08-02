using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using test_api.Models.Responses;

namespace test_api.Services
{
    public interface IAdminService
    {
        Task<AdminStatsDto> GetStatsAsync();
        Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(PaginationDto pagination);
        Task<PagedResultDto<ProductDto>> GetAllProductsAsync(PaginationDto pagination);
        Task<PagedResultDto<UserResponseDto>> GetAllUsersAsync(PaginationDto pagination);
    }

    public class AdminService : IAdminService
    {
        private readonly StoreContext _context;

        public AdminService(StoreContext context)
        {
            _context = context;
        }

        public async Task<AdminStatsDto> GetStatsAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");
            var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");

            // Monthly revenue for last 6 months
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyRevenue = await _context.Orders
                .Where(o => o.OrderDate >= sixMonthsAgo && o.Status == "Delivered")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToListAsync();

            // Top 5 products by revenue
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.Status == "Delivered")
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToListAsync();

            return new AdminStatsDto
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalCategories = totalCategories,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                MonthlyRevenue = monthlyRevenue,
                TopProducts = topProducts
            };
        }

        public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(PaginationDto pagination)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(o => o.CustomerName.Contains(pagination.Search) || 
                                       o.CustomerEmail.Contains(pagination.Search) ||
                                       o.Status.Contains(pagination.Search) ||
                                       o.User.Name.Contains(pagination.Search));
            }

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "date_asc" => query.OrderBy(o => o.OrderDate),
                    "date_desc" => query.OrderByDescending(o => o.OrderDate),
                    "amount_asc" => query.OrderBy(o => o.TotalAmount),
                    "amount_desc" => query.OrderByDescending(o => o.TotalAmount),
                    "status_asc" => query.OrderBy(o => o.Status),
                    "status_desc" => query.OrderByDescending(o => o.Status),
                    _ => query.OrderByDescending(o => o.OrderDate)
                };
            }
            else
            {
                query = query.OrderByDescending(o => o.OrderDate);
            }

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                UserId = o.UserId,
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl
                }).ToList()
            }).ToList();

            return new PagedResultDto<OrderDto>
            {
                Data = orderDtos,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }

        public async Task<PagedResultDto<ProductDto>> GetAllProductsAsync(PaginationDto pagination)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => p.Name.Contains(pagination.Search) || 
                                       p.Description.Contains(pagination.Search) ||
                                       p.Category.Name.Contains(pagination.Search) ||
                                       p.Store.Name.Contains(pagination.Search));
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

        public async Task<PagedResultDto<UserResponseDto>> GetAllUsersAsync(PaginationDto pagination)
        {
            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(u => u.Name.Contains(pagination.Search) || 
                                       u.Email.Contains(pagination.Search));
            }

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "username_asc" => query.OrderBy(u => u.Name),
                    "username_desc" => query.OrderByDescending(u => u.Name),
                    "email_asc" => query.OrderBy(u => u.Email),
                    "email_desc" => query.OrderByDescending(u => u.Email),
                    "date_asc" => query.OrderBy(u => u.CreatedAt),
                    "date_desc" => query.OrderByDescending(u => u.CreatedAt),
                    _ => query.OrderByDescending(u => u.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsAdmin = false, // Assuming we don't expose IsAdmin in the response
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return new PagedResultDto<UserResponseDto>
            {
                Data = users,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }
    }
}
