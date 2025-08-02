using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_api.Models.Dtos;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResultDto<CategoryDto>>>> GetCategories([FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _categoryService.GetCategoriesAsync(pagination);
                return Ok(new ApiResponse<PagedResultDto<CategoryDto>>
                {
                    Success = true,
                    Data = result,
                    Message = "Categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PagedResultDto<CategoryDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Data = category,
                    Message = "Category retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Data = category,
                    Message = "Category created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
                if (category == null)
                {
                    return NotFound(new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Category not found"
                    });
                }

                return Ok(new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Data = category,
                    Message = "Category updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Category not found or has associated products"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Category deleted successfully"
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
