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
    public class SellerController : ControllerBase
    {
        private readonly ISellerService _sellerService;

        public SellerController(ISellerService sellerService)
        {
            _sellerService = sellerService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // Store Management
        [HttpGet("store")]
        public async Task<ActionResult<ApiResponse<StoreDto>>> GetSellerStore()
        {
            try
            {
                var sellerId = GetUserId();
                var store = await _sellerService.GetSellerStoreAsync(sellerId);
                
                if (store == null)
                {
                    return NotFound(new ApiResponse<StoreDto>
                    {
                        Success = false,
                        Message = "Store not found"
                    });
                }

                return Ok(new ApiResponse<StoreDto>
                {
                    Success = true,
                    Data = store,
                    Message = "Store retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StoreDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("store")]
        public async Task<ActionResult<ApiResponse<StoreDto>>> CreateStore([FromBody] CreateStoreDto createStoreDto)
        {
            try
            {
                var sellerId = GetUserId();
                var store = await _sellerService.CreateStoreAsync(sellerId, createStoreDto);
                
                return CreatedAtAction(nameof(GetSellerStore), new ApiResponse<StoreDto>
                {
                    Success = true,
                    Data = store,
                    Message = "Store created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StoreDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("store/{storeId}")]
        public async Task<ActionResult<ApiResponse<StoreDto>>> UpdateStore(int storeId, [FromBody] UpdateStoreDto updateStoreDto)
        {
            try
            {
                var sellerId = GetUserId();
                var store = await _sellerService.UpdateStoreAsync(sellerId, storeId, updateStoreDto);
                
                if (store == null)
                {
                    return NotFound(new ApiResponse<StoreDto>
                    {
                        Success = false,
                        Message = "Store not found"
                    });
                }

                return Ok(new ApiResponse<StoreDto>
                {
                    Success = true,
                    Data = store,
                    Message = "Store updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StoreDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // Product Management
        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ProductDto>>>> GetSellerProducts([FromQuery] PaginationDto pagination)
        {
            try
            {
                var sellerId = GetUserId();
                var result = await _sellerService.GetSellerProductsAsync(sellerId, pagination);
                
                return Ok(new ApiResponse<PagedResultDto<ProductDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Products retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<ProductDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("products/{productId}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetSellerProduct(int productId)
        {
            try
            {
                var sellerId = GetUserId();
                var product = await _sellerService.GetSellerProductByIdAsync(sellerId, productId);
                
                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Product retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("products")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateSellerProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                var sellerId = GetUserId();
                var product = await _sellerService.CreateSellerProductAsync(sellerId, createProductDto);
                
                return CreatedAtAction(nameof(GetSellerProduct), new { productId = product.Id }, new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Product created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("products/{productId}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateSellerProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                var sellerId = GetUserId();
                var product = await _sellerService.UpdateSellerProductAsync(sellerId, productId, updateProductDto);
                
                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Product updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("products/{productId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSellerProduct(int productId)
        {
            try
            {
                var sellerId = GetUserId();
                var result = await _sellerService.DeleteSellerProductAsync(sellerId, productId);
                
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
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

        // Order Management
        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<OrderDto>>>> GetSellerOrders([FromQuery] PaginationDto pagination)
        {
            try
            {
                var sellerId = GetUserId();
                var result = await _sellerService.GetSellerOrdersAsync(sellerId, pagination);
                
                return Ok(new ApiResponse<PagedResultDto<OrderDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Orders retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<OrderDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("orders/{orderId}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetSellerOrder(int orderId)
        {
            try
            {
                var sellerId = GetUserId();
                var order = await _sellerService.GetSellerOrderByIdAsync(sellerId, orderId);
                
                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Order retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateSellerOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            try
            {
                var sellerId = GetUserId();
                var order = await _sellerService.UpdateSellerOrderStatusAsync(sellerId, orderId, updateOrderStatusDto);
                
                if (order == null)
                {
                    return NotFound(new ApiResponse<OrderDto>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }

                return Ok(new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Order status updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // Statistics
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<SellerStatsDto>>> GetSellerStats()
        {
            try
            {
                var sellerId = GetUserId();
                var stats = await _sellerService.GetSellerStatsAsync(sellerId);
                
                return Ok(new ApiResponse<SellerStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<SellerStatsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
