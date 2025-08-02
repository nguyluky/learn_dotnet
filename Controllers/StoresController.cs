using Microsoft.AspNetCore.Mvc;
using test_api.Models.Dtos;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoresController : ControllerBase
    {
        private readonly ISellerService _sellerService;
        private readonly IProductService _productService;

        public StoresController(ISellerService sellerService, IProductService productService)
        {
            _sellerService = sellerService;
            _productService = productService;
        }

        [HttpGet("{storeId}")]
        public async Task<ActionResult<ApiResponse<StoreDto>>> GetStore(int storeId)
        {
            try
            {
                var store = await _sellerService.GetStoreByIdAsync(storeId);
                
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

        [HttpGet("{storeId}/products")]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ProductDto>>>> GetStoreProducts(int storeId, [FromQuery] PaginationDto pagination)
        {
            try
            {
                // Verify store exists
                var store = await _sellerService.GetStoreByIdAsync(storeId);
                if (store == null)
                {
                    return NotFound(new ApiResponse<PagedResultDto<ProductDto>>
                    {
                        Success = false,
                        Message = "Store not found"
                    });
                }

                // Get products for this store (we'll modify ProductService for this)
                var result = await _productService.GetProductsByStoreAsync(storeId, pagination);
                
                return Ok(new ApiResponse<PagedResultDto<ProductDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Store products retrieved successfully"
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
    }
}
