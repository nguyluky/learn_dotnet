
using Microsoft.AspNetCore.Mvc;
using test_api.Helpers;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using test_api.Models.Responses;
using test_api.Services;

namespace test_api.controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IAuthService _authService;
        private readonly UserService _userService;
        private readonly JwtHelper _jwtHelper;

        public UserController(ILogger<UserController> logger, IAuthService authService, JwtHelper jwtHelper, UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
        }

        // Define your endpoints here
        [HttpGet("me")]
        public async Task<ActionResult<ActionResult<UserResponse>>> GetCurrentUser()
        {
            var userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }


            var user = await _userService.GetUserByIdAsync(int.Parse(userId));

            return Ok(ApiResponse<UserResponse>.SuccessResponse(new UserResponse
            {
                Id = user!.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Rule = user.Rule?.Name // Assuming Rule is a navigation property
            }));
        }

        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateCurrentUser([FromBody] UserUpdateDto userUpdateRequest)
        {
            var userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(int.Parse(userId), userUpdateRequest);

                return Ok(ApiResponse<UserResponse>.SuccessResponse(
                    new UserResponse
                    {
                        Id = updatedUser!.Id,
                        Name = updatedUser.Name,
                        Email = updatedUser.Email,
                        Phone = updatedUser.Phone,
                        Rule = updatedUser.Rule?.Name // Assuming Rule is a navigation property
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current user.");
                return StatusCode(500, "Internal server error.");
            }
        }
        // [HttpGet("/{id:int}", Name = "GetUserById")]
        // public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id)
        // {
        //     var user = await _userService.GetUserByIdAsync(id);
        //     if (user == null)
        //     {
        //         return NotFound(ApiResponse<UserResponse>.ErrorResponse("User not found."));
        //     }
        //
        //     var userResponse = new UserResponse
        //     {
        //         Id = user.Id,
        //         Name = user.Name,
        //         Email = user.Email,
        //         Phone = user.Phone,
        //         Rule = user.Rule?.Name // Assuming Rule is a navigation property
        //     };
        //
        //     return Ok(ApiResponse<UserResponse>.SuccessResponse(userResponse));
        // }
        //
        // [HttpGet()]
        // public async Task<ActionResult<ApiResponse<PaddingResponse<UserResponse>>>> GetAllUsers([FromQuery] SimplePaddingRequest simplePaddingRequest)
        // {
        //     try
        //     {
        //         var users = await _userService.GetAllUsersHavePaddingAsync(
        //             padding: simplePaddingRequest.Padding,
        //             limit: simplePaddingRequest.Limit);
        //
        //         var userResponses = users.Select(static user => new UserResponse
        //         {
        //             Id = user.Id,
        //             Name = user.Name,
        //             Email = user.Email,
        //             Phone = user.Phone,
        //             Rule = user.Rule?.Name // Assuming Rule is a navigation property
        //         }).ToList();
        //
        //         return Ok(ApiResponse<PaddingResponse<UserResponse>>.SuccessResponse(
        //             new PaddingResponse<UserResponse>
        //             {
        //                 Padding = simplePaddingRequest.Padding,
        //                 Limit = simplePaddingRequest.Limit,
        //                 Items = userResponses,
        //                 TotalCount = users.Count
        //             }));
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error retrieving all users.");
        //         return StatusCode(500, "Internal server error.");
        //     }
        // }
    }

}
