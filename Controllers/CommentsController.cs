using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using test_api.Models.Dtos;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<CommentDto>>>> GetProductComments(int productId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _commentService.GetProductCommentsAsync(productId, pagination);
                return Ok(new ApiResponse<PagedResultDto<CommentDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Comments retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<CommentDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(int productId, [FromBody] CreateCommentDto createCommentDto)
        {
            try
            {
                // Ensure productId matches route parameter
                createCommentDto.ProductId = productId;
                
                var userId = GetUserId();
                var comment = await _commentService.CreateCommentAsync(userId, createCommentDto);
                
                return CreatedAtAction(nameof(GetComment), new { productId, id = comment.Id }, new ApiResponse<CommentDto>
                {
                    Success = true,
                    Data = comment,
                    Message = "Comment created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CommentDto>>> GetComment(int productId, int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null || comment.ProductId != productId)
                {
                    return NotFound(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Comment not found"
                    });
                }

                return Ok(new ApiResponse<CommentDto>
                {
                    Success = true,
                    Data = comment,
                    Message = "Comment retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CommentDto>>> UpdateComment(int productId, int id, [FromBody] UpdateCommentDto updateCommentDto)
        {
            try
            {
                var userId = GetUserId();
                var comment = await _commentService.UpdateCommentAsync(id, userId, updateCommentDto);
                
                if (comment == null)
                {
                    return NotFound(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Comment not found or you don't have permission to update it"
                    });
                }

                return Ok(new ApiResponse<CommentDto>
                {
                    Success = true,
                    Data = comment,
                    Message = "Comment updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteComment(int productId, int id)
        {
            try
            {
                var userId = GetUserId();
                var isAdmin = IsAdmin();
                var result = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
                
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Comment not found or you don't have permission to delete it"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Comment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
