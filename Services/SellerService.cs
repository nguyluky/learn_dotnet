using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using Mapster;

namespace test_api.Services
{
    public interface ISellerService
    {
        // Store Management
        Task<StoreDto?> GetStoreByIdAsync(int storeId);
        Task<StoreDto?> GetSellerStoreAsync(int sellerId);
        Task<StoreDto> CreateStoreAsync(int sellerId, CreateStoreDto createStoreDto);
        Task<StoreDto?> UpdateStoreAsync(int sellerId, int storeId, UpdateStoreDto updateStoreDto);
        
        // Product Management
        Task<PagedResultDto<ProductDto>> GetSellerProductsAsync(int sellerId, PaginationDto pagination);
        Task<ProductDto?> GetSellerProductByIdAsync(int sellerId, int productId);
        Task<ProductDto> CreateSellerProductAsync(int sellerId, CreateProductDto createProductDto);
        Task<ProductDto?> UpdateSellerProductAsync(int sellerId, int productId, UpdateProductDto updateProductDto);
        Task<bool> DeleteSellerProductAsync(int sellerId, int productId);
        
        // Order Management
        Task<PagedResultDto<OrderDto>> GetSellerOrdersAsync(int sellerId, PaginationDto pagination);
        Task<OrderDto?> GetSellerOrderByIdAsync(int sellerId, int orderId);
        Task<OrderDto?> UpdateSellerOrderStatusAsync(int sellerId, int orderId, UpdateOrderStatusDto updateOrderStatusDto);
        
        // Statistics
        Task<SellerStatsDto> GetSellerStatsAsync(int sellerId);
    }

    public class SellerService : ISellerService
    {
        private readonly StoreContext _context;

        public SellerService(StoreContext context)
        {
            _context = context;
        }

        // Store Management
        public async Task<StoreDto?> GetStoreByIdAsync(int storeId)
        {
            var store = await _context.Store
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store == null)
                return null;

            var productCount = await _context.Products.CountAsync(p => p.StoreId == storeId);

            return new StoreDto
            {
                Id = store.Id,
                Name = store.Name,
                Location = store.Location,
                Description = store.Description,
                OwnerId = store.OwnerId,
                OwnerName = store.Owner.Name,
                Avatar = store.avtar,
                Banner = store.banner,
                CreatedAt = store.CreatedAt,
                UpdatedAt = store.UpdatedAt,
                ProductCount = productCount
            };
        }

        public async Task<StoreDto?> GetSellerStoreAsync(int sellerId)
        {
            var store = await _context.Store
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.OwnerId == sellerId);

            if (store == null)
                return null;

            return await GetStoreByIdAsync(store.Id);
        }

        public async Task<StoreDto> CreateStoreAsync(int sellerId, CreateStoreDto createStoreDto)
        {
            // Check if seller already has a store
            var existingStore = await _context.Store
                .FirstOrDefaultAsync(s => s.OwnerId == sellerId);

            if (existingStore != null)
                throw new InvalidOperationException("Seller already has a store");

            var store = new Store
            {
                Name = createStoreDto.Name,
                Location = createStoreDto.Location,
                Description = createStoreDto.Description,
                OwnerId = sellerId,
                avtar = createStoreDto.Avatar,
                banner = createStoreDto.Banner,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Store.Add(store);
            await _context.SaveChangesAsync();

            return await GetStoreByIdAsync(store.Id) ?? throw new InvalidOperationException("Failed to create store");
        }

        public async Task<StoreDto?> UpdateStoreAsync(int sellerId, int storeId, UpdateStoreDto updateStoreDto)
        {
            var store = await _context.Store
                .FirstOrDefaultAsync(s => s.Id == storeId && s.OwnerId == sellerId);

            if (store == null)
                return null;

            if (!string.IsNullOrEmpty(updateStoreDto.Name))
                store.Name = updateStoreDto.Name;
            if (!string.IsNullOrEmpty(updateStoreDto.Location))
                store.Location = updateStoreDto.Location;
            if (!string.IsNullOrEmpty(updateStoreDto.Description))
                store.Description = updateStoreDto.Description;
            if (!string.IsNullOrEmpty(updateStoreDto.Avatar))
                store.avtar = updateStoreDto.Avatar;
            if (!string.IsNullOrEmpty(updateStoreDto.Banner))
                store.banner = updateStoreDto.Banner;

            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetStoreByIdAsync(storeId);
        }

        // Product Management
        public async Task<PagedResultDto<ProductDto>> GetSellerProductsAsync(int sellerId, PaginationDto pagination)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                throw new InvalidOperationException("Seller does not have a store");

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Where(p => p.StoreId == store.Id)
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

        public async Task<ProductDto?> GetSellerProductByIdAsync(int sellerId, int productId)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                return null;

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == store.Id);

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

        public async Task<ProductDto> CreateSellerProductAsync(int sellerId, CreateProductDto createProductDto)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                throw new InvalidOperationException("Seller does not have a store");

            var product = createProductDto.Adapt<Product>();
            product.StoreId = store.Id;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return await GetSellerProductByIdAsync(sellerId, product.Id) ?? throw new InvalidOperationException("Failed to create product");
        }

        public async Task<ProductDto?> UpdateSellerProductAsync(int sellerId, int productId, UpdateProductDto updateProductDto)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                return null;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == store.Id);

            if (product == null)
                return null;

            updateProductDto.Adapt(product);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetSellerProductByIdAsync(sellerId, productId);
        }

        public async Task<bool> DeleteSellerProductAsync(int sellerId, int productId)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                return false;

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == store.Id);

            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // Order Management
        public async Task<PagedResultDto<OrderDto>> GetSellerOrdersAsync(int sellerId, PaginationDto pagination)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                throw new InvalidOperationException("Seller does not have a store");

            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Items.Any(oi => oi.Product.StoreId == store.Id))
                .AsQueryable();

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
                Items = o.Items.Where(oi => oi.Product.StoreId == store.Id).Select(oi => new OrderItemDto
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

        public async Task<OrderDto?> GetSellerOrderByIdAsync(int sellerId, int orderId)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                return null;

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == orderId && o.Items.Any(oi => oi.Product.StoreId == store.Id))
                .FirstOrDefaultAsync();

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
                Items = order.Items.Where(oi => oi.Product.StoreId == store.Id).Select(oi => new OrderItemDto
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

        public async Task<OrderDto?> UpdateSellerOrderStatusAsync(int sellerId, int orderId, UpdateOrderStatusDto updateOrderStatusDto)
        {
            var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            if (store == null)
                return null;

            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.Items.Any(oi => oi.Product.StoreId == store.Id))
                .FirstOrDefaultAsync();

            if (order == null)
                return null;

            order.Status = updateOrderStatusDto.Status;
            await _context.SaveChangesAsync();

            return await GetSellerOrderByIdAsync(sellerId, orderId);
        }

        // Statistics
        public async Task<SellerStatsDto> GetSellerStatsAsync(int sellerId)
        {
            // var store = await _context.Store.FirstOrDefaultAsync(s => s.OwnerId == sellerId);
            // if (store == null)
            //     throw new InvalidOperationException("Seller does not have a store");
            //
            // var totalProducts = await _context.Products.CountAsync(p => p.StoreId == store.Id);
            // 
            // var orderQuery = _context.Orders
            //     .Include(o => o.Items)
            //     .Where(o => o.Items.Any(oi => oi.Product.StoreId == store.Id));
            //
            // var totalOrders = await orderQuery.CountAsync();
            // 
            // var totalRevenue = await orderQuery
            //     .Where(o => o.Status == "Delivered")
            //     .SelectMany(o => o.Items)
            //     .Where(oi => oi.Product.StoreId == store.Id)
            //     .SumAsync(oi => oi.Price * oi.Quantity);
            //
            // var pendingOrders = await orderQuery.CountAsync(o => o.Status == "Pending");
            // var processingOrders = await orderQuery.CountAsync(o => o.Status == "Processing");
            // var deliveredOrders = await orderQuery.CountAsync(o => o.Status == "Delivered");
            //
            // // Monthly revenue for last 6 months
            // var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            // var monthlyRevenue = await _context.Orders
            //     .Include(o => o.Items)
            //     .ThenInclude(oi => oi.Product)
            //     .Where(o => o.OrderDate >= sixMonthsAgo && 
            //               o.Status == "Delivered" && 
            //               o.Items.Any(oi => oi.Product.StoreId == store.Id))
            //     .SelectMany(o => o.Items.Where(oi => oi.Product.StoreId == store.Id)
            //         .Select(oi => new { o.OrderDate, Revenue = oi.Price * oi.Quantity }))
            //     .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
            //     .Select(g => new MonthlySellerRevenueDto
            //     {
            //         Month = $"{g.Key.Year}-{g.Key.Month:D2}",
            //         Revenue = g.Sum(x => x.Revenue),
            //         OrderCount = g.Count()
            //     })
            //     .OrderBy(m => m.Month)
            //     .ToListAsync();
            //
            // // Top 5 products by revenue
            // var topProducts = await _context.OrderItems
            //     .Include(oi => oi.Order)
            //     .Include(oi => oi.Product)
            //     .Where(oi => oi.Order.Status == "Delivered" && oi.Product.StoreId == store.Id)
            //     .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            //     .Select(g => new TopSellerProductDto
            //     {
            //         ProductId = g.Key.ProductId,
            //         ProductName = g.Key.Name,
            //         TotalSold = g.Sum(oi => oi.Quantity),
            //         Revenue = g.Sum(oi => oi.Price * oi.Quantity)
            //     })
            //     .OrderByDescending(p => p.Revenue)
            //     .Take(5)
            //     .ToListAsync();
            //
            // return new SellerStatsDto
            // {
            //     TotalProducts = totalProducts,
            //     TotalOrders = totalOrders,
            //     TotalRevenue = totalRevenue,
            //     PendingOrders = pendingOrders,
            //     ProcessingOrders = processingOrders,
            //     DeliveredOrders = deliveredOrders,
            //     MonthlyRevenue = monthlyRevenue,
            //     TopProducts = topProducts
            // };
            
            return new SellerStatsDto
            {
                TotalProducts = 0,
                TotalOrders = 0,
                TotalRevenue = 0,
                PendingOrders = 0,
                ProcessingOrders = 0,
                DeliveredOrders = 0,
                MonthlyRevenue = new List<MonthlySellerRevenueDto>(),
                TopProducts = new List<TopSellerProductDto>()
            };
        }
    }
}
