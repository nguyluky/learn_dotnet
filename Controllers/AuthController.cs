
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test_api.Exceptions;
using test_api.Helpers;
using test_api.Models.Dtos;
using test_api.Models.Entities;
using test_api.Models.Responses;
using test_api.Services;
namespace test_api.controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(ILogger<AuthController> logger, IAuthService authService, JwtHelper jwtHelper) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _authService = authService;
        private readonly JwtHelper _jwtHelper = jwtHelper;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] RegisterRequest user)
        {
            if (user == null)
            {
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Invalid registration data."));
            }

            // Check if the email already exists
            if (await _authService.EmailExistsAsync(user.Email))
            {
                return Conflict(ApiResponse<UserResponse>.ErrorResponse("Email already exists."));
            }

            int role = await _authService.GetDefaultRoleId();


            User createdUser = await _authService.RegisterAsync(
                user.Name, user.Email, user.Phone, user.Password, role
            );
            return CreatedAtRoute("GetUserById", new { id = createdUser.Id },
                ApiResponse<UserResponse>.SuccessResponse(new UserResponse
                {
                    Id = createdUser.Id,
                    Name = createdUser.Name,
                    Email = createdUser.Email,
                    Phone = createdUser.Phone,
                    Rule = createdUser.Rule?.Name ?? "User",
                }, "User registered successfully.")
            );
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse("Invalid login data."));
            }

            var user = await _authService.LoginAsync(loginRequest.Email, loginRequest.Password);
            if (user == null)
            {
                return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse("Invalid email or password."));
            }

            // Here you would typically generate a JWT token and return it
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken(user);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "User logged in successfully."));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromQuery] String refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Refresh token is required."));
            }

            var principal = _jwtHelper.ValidateToken(refreshToken);
            if (principal == null)
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid refresh token."));
            }

            var userId = int.Parse(principal.FindFirst("id")?.Value ?? "0");
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            return Ok(new { AccessToken = newAccessToken });
        }

    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;
    }


}
