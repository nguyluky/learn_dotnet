using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using test_api.Models.Dtos;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                
                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("items")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
                
                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Product added to cart successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.UpdateCartItemAsync(userId, itemId, updateCartItemDto);
                
                if (cart == null)
                {
                    return NotFound(new ApiResponse<CartDto>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                return Ok(new ApiResponse<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart item updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CartDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveFromCart(int itemId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.RemoveFromCartAsync(userId, itemId);
                
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cart item not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product removed from cart successfully"
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

        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var result = await _cartService.ClearCartAsync(userId);
                
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cart not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Cart cleared successfully"
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
