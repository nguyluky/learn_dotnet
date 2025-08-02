using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;

namespace test_api.Services
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderDto>> GetOrdersAsync(int? userId, PaginationDto pagination);
        Task<OrderDto?> GetOrderByIdAsync(int id, int? userId = null);
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);
        Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateOrderStatusDto);
        Task<bool> CancelOrderAsync(int id, int userId);
    }

    public class OrderService : IOrderService
    {
        private readonly StoreContext _context;

        public OrderService(StoreContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<OrderDto>> GetOrdersAsync(int? userId, PaginationDto pagination)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsQueryable();

            // Filter by user if specified
            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            // Search
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(o => o.CustomerName.Contains(pagination.Search) || 
                                       o.CustomerEmail.Contains(pagination.Search) ||
                                       o.Status.Contains(pagination.Search));
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

        public async Task<OrderDto?> GetOrderByIdAsync(int id, int? userId = null)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Where(o => o.Id == id);

            if (userId.HasValue)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            var order = await query.FirstOrDefaultAsync();

            if (order == null)
                return null;

            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                UserId = order.UserId,
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImageUrl = oi.Product.ImageUrl
                }).ToList()
            };
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
        {
            // Get user's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                CustomerName = createOrderDto.CustomerName,
                CustomerEmail = createOrderDto.CustomerEmail,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Items = cart.Items.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(oi => oi.Price * oi.Quantity);

            _context.Orders.Add(order);
            
            // Clear cart after creating order
            _context.CartItems.RemoveRange(cart.Items);
            
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to create order");
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto updateOrderStatusDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return null;

            order.Status = updateOrderStatusDto.Status;
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(id);
        }

        public async Task<bool> CancelOrderAsync(int id, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null || order.Status == "Cancelled" || order.Status == "Delivered")
                return false;

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
