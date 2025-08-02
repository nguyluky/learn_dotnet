using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_api.Models.Dtos;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IOrderService _orderService;

        public AdminController(IAdminService adminService, IOrderService orderService)
        {
            _adminService = adminService;
            _orderService = orderService;
        }

        [HttpGet("stats", Name = "GetAdminStats")]
        public async Task<ActionResult<ApiResponse<AdminStatsDto>>> GetStats()
        {
            try
            {
                var stats = await _adminService.GetStatsAsync();
                return Ok(new ApiResponse<AdminStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<AdminStatsDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("orders")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<OrderDto>>>> GetAllOrders([FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _adminService.GetAllOrdersAsync(pagination);
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

        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ProductDto>>>> GetAllProducts([FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _adminService.GetAllProductsAsync(pagination);
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

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<UserResponseDto>>>> GetAllUsers([FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync(pagination);
                return Ok(new ApiResponse<PagedResultDto<UserResponseDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<UserResponseDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("orders/{id}/status")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto);
                
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
    }
}
