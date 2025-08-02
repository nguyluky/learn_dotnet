using Microsoft.EntityFrameworkCore;
using test_api.Data;
using test_api.Models.Dtos;
using test_api.Models.Entities;

namespace test_api.Services
{
    public interface ICommentService
    {
        Task<PagedResultDto<CommentDto>> GetProductCommentsAsync(int productId, PaginationDto pagination);
        Task<CommentDto?> GetCommentByIdAsync(int id);
        Task<CommentDto> CreateCommentAsync(int userId, CreateCommentDto createCommentDto);
        Task<CommentDto?> UpdateCommentAsync(int id, int userId, UpdateCommentDto updateCommentDto);
        Task<bool> DeleteCommentAsync(int id, int userId, bool isAdmin = false);
    }

    public class CommentService : ICommentService
    {
        private readonly StoreContext _context;

        public CommentService(StoreContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<CommentDto>> GetProductCommentsAsync(int productId, PaginationDto pagination)
        {
            var query = _context.Comments
                .Include(c => c.User)
                .Where(c => c.ProductId == productId)
                .AsQueryable();

            // Sort
            if (!string.IsNullOrEmpty(pagination.Sort))
            {
                query = pagination.Sort.ToLower() switch
                {
                    "rating_asc" => query.OrderBy(c => c.Rating),
                    "rating_desc" => query.OrderByDescending(c => c.Rating),
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
            var comments = await query
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    ProductId = c.ProductId,
                    OrderItemId = c.OrderItemId
                })
                .ToListAsync();

            return new PagedResultDto<CommentDto>
            {
                Data = comments,
                TotalCount = totalCount,
                Page = pagination.Page,
                Limit = pagination.Limit
            };
        }

        public async Task<CommentDto?> GetCommentByIdAsync(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Rating = c.Rating,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    ProductId = c.ProductId,
                    OrderItemId = c.OrderItemId
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            return comment;
        }

        public async Task<CommentDto> CreateCommentAsync(int userId, CreateCommentDto createCommentDto)
        {
            // Check if user has purchased this product
            var hasOrderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.Id == createCommentDto.OrderItemId && 
                              oi.ProductId == createCommentDto.ProductId &&
                              oi.Order.UserId == userId &&
                              oi.Order.Status == "Delivered");

            if (!hasOrderItem)
                throw new InvalidOperationException("You can only comment on products you have purchased and received");

            // Check if comment already exists for this order item
            var existingComment = await _context.Comments
                .FirstOrDefaultAsync(c => c.OrderItemId == createCommentDto.OrderItemId && c.UserId == userId);

            if (existingComment != null)
                throw new InvalidOperationException("You have already commented on this product");

            var comment = new Comment
            {
                Content = createCommentDto.Content,
                Rating = createCommentDto.Rating,
                UserId = userId,
                ProductId = createCommentDto.ProductId,
                OrderItemId = createCommentDto.OrderItemId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return await GetCommentByIdAsync(comment.Id) ?? throw new InvalidOperationException("Failed to create comment");
        }

        public async Task<CommentDto?> UpdateCommentAsync(int id, int userId, UpdateCommentDto updateCommentDto)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (comment == null)
                return null;

            if (!string.IsNullOrEmpty(updateCommentDto.Content))
                comment.Content = updateCommentDto.Content;

            if (updateCommentDto.Rating.HasValue)
                comment.Rating = updateCommentDto.Rating.Value;

            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetCommentByIdAsync(id);
        }

        public async Task<bool> DeleteCommentAsync(int id, int userId, bool isAdmin = false)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return false;

            // Only comment owner or admin can delete
            if (!isAdmin && comment.UserId != userId)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
