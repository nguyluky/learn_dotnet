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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<OrderDto>>>> GetOrders([FromQuery] PaginationDto pagination)
        {
            try
            {
                var userId = IsAdmin() ? (int?)null : GetUserId();
                var result = await _orderService.GetOrdersAsync(userId, pagination);
                
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

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
        {
            try
            {
                var userId = IsAdmin() ? (int?)null : GetUserId();
                var order = await _orderService.GetOrderByIdAsync(id, userId);
                
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
                
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new ApiResponse<OrderDto>
                {
                    Success = true,
                    Data = order,
                    Message = "Order created successfully"
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

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<object>>> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _orderService.CancelOrderAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Order not found or cannot be cancelled"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Order cancelled successfully"
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

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
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
