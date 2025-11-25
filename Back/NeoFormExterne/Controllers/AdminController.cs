using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using System.Security.Claims;

namespace NeoFormExterne.Controllers
{
    [ApiController]
    [Route("neoformexternal/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        // POST: /Admin/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _adminService.LoginAsync(request.Username, request.Password);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                return Ok(new
                {
                    token = result.Token,
                    username = result.Username,
                    roles = result.Roles,
                    fullName = result.FullName,
                    mustChangePassword = result.MustChangePassword
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.Login: Unexpected error during login for '{Username}'", request.Username);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // POST: /Admin/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var result = await _adminService.ChangePasswordAsync(userId!, request.CurrentPassword, request.NewPassword);

                if (!result.Success)
                {
                    if (result.Errors != null && result.Errors.Any())
                    {
                        return BadRequest(new { errors = result.Errors });
                    }
                    return NotFound(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.ChangePassword: Unexpected error");
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        // POST: /Admin/create-admin (SuperAdmin only)
        [HttpPost("create-admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var createdBy = User.Identity?.Name ?? "Unknown";

            try
            {
                var result = await _adminService.CreateAdminAsync(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.FullName,
                    createdBy);

                if (!result.Success)
                {
                    if (result.Errors != null && result.Errors.Any())
                    {
                        return BadRequest(new { errors = result.Errors });
                    }
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new
                {
                    message = result.Message,
                    username = result.Username,
                    role = result.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.CreateAdmin: Unexpected error creating admin user '{Username}'", request.Username);
                return StatusCode(500, new { message = "An error occurred while creating admin user" });
            }
        }

        // GET: /Admin/users (SuperAdmin only)
        [HttpGet("users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.GetUsers: Error retrieving users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        // DELETE: /Admin/users/{userId} (SuperAdmin only)
        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var result = await _adminService.DeleteUserAsync(userId);

                if (!result.Success)
                {
                    if (result.Message == "User not found")
                    {
                        return NotFound(new { message = result.Message });
                    }
                    if (result.Errors != null && result.Errors.Any())
                    {
                        return BadRequest(new { errors = result.Errors });
                    }
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.DeleteUser: Unexpected error deleting user ID '{UserId}'", userId);
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }
    }

    // Request models
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }
}
