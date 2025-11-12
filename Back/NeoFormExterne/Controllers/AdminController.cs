using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NeoFormExterne.Controllers
{
    [ApiController]
    [Route("neoformexternal/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: /Admin/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("🔐 AdminController.Login: Login attempt for username '{Username}'", request.Username);

            try
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminController.Login: User not found: '{Username}'", request.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                _logger.LogInformation("🔐 AdminController.Login: User found, checking password for '{Username}'", request.Username);
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("⚠️ AdminController.Login: Account locked out for '{Username}'", request.Username);
                        return Unauthorized(new { message = "Account is locked. Please try again later." });
                    }

                    _logger.LogWarning("⚠️ AdminController.Login: Invalid password for '{Username}'", request.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("🔐 AdminController.Login: Password verified for '{Username}', Roles: {Roles}", request.Username, string.Join(", ", roles));

                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation("✅ AdminController.Login: Login successful for '{Username}', MustChangePassword: {MustChange}",
                    request.Username, user.MustChangePassword);

                return Ok(new
                {
                    token,
                    username = user.UserName,
                    roles,
                    fullName = user.FullName,
                    mustChangePassword = user.MustChangePassword // Flag for front-end
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
            var username = User.Identity?.Name ?? "Unknown";

            _logger.LogInformation("🔑 AdminController.ChangePassword: Password change request for user '{Username}' (ID: {UserId})", username, userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId!);

                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminController.ChangePassword: User not found for ID '{UserId}'", userId);
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation("🔑 AdminController.ChangePassword: Attempting to change password for '{Username}'", user.UserName);
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminController.ChangePassword: Password change failed for '{Username}': {Errors}",
                        user.UserName, errors);
                    return BadRequest(new { errors = result.Errors });
                }

                // Remove the must change password flag
                var wasForcedChange = user.MustChangePassword;
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("✅ AdminController.ChangePassword: Password changed successfully for '{Username}', ForcedChange: {WasForced}",
                    user.UserName, wasForcedChange);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.ChangePassword: Unexpected error for user '{Username}'", username);
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        // POST: /Admin/create-admin (SuperAdmin only)
        [HttpPost("create-admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var createdBy = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation("👤 AdminController.CreateAdmin: Request to create admin user '{Username}' by '{CreatedBy}'",
                request.Username, createdBy);

            try
            {
                var existingUser = await _userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("⚠️ AdminController.CreateAdmin: Username '{Username}' already exists", request.Username);
                    return BadRequest(new { message = "Username already exists" });
                }

                _logger.LogInformation("👤 AdminController.CreateAdmin: Creating new admin user '{Username}'", request.Username);

                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    CreatedBy = createdBy
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminController.CreateAdmin: Failed to create user '{Username}': {Errors}",
                        request.Username, errors);
                    return BadRequest(new { errors = result.Errors });
                }

                _logger.LogInformation("👤 AdminController.CreateAdmin: User '{Username}' created, assigning Admin role", request.Username);

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    _logger.LogInformation("👤 AdminController.CreateAdmin: Creating Admin role");
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Assign Admin role
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("✅ AdminController.CreateAdmin: Admin user '{Username}' created successfully by '{CreatedBy}'",
                        request.Username, createdBy);
                }
                else
                {
                    _logger.LogWarning("⚠️ AdminController.CreateAdmin: User created but role assignment failed for '{Username}'",
                        request.Username);
                }

                return Ok(new
                {
                    message = "Admin user created successfully",
                    username = user.UserName,
                    role = "Admin"
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
            var requestedBy = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation("📋 AdminController.GetUsers: User list requested by '{RequestedBy}'", requestedBy);

            try
            {
                var users = _userManager.Users.Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FullName,
                    u.CreatedAt,
                    u.CreatedBy
                }).ToList();

                // Get roles for each user
                var usersWithRoles = new List<object>();
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.Id);
                    var roles = await _userManager.GetRolesAsync(appUser!);
                    usersWithRoles.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FullName,
                        user.CreatedAt,
                        user.CreatedBy,
                        Roles = roles
                    });
                }

                _logger.LogInformation("✅ AdminController.GetUsers: Returned {Count} users to '{RequestedBy}'", users.Count, requestedBy);
                return Ok(usersWithRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.GetUsers: Error retrieving users for '{RequestedBy}'", requestedBy);
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        // DELETE: /Admin/users/{userId} (SuperAdmin only)
        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var requestedBy = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation("🗑️ AdminController.DeleteUser: Delete request for user ID '{UserId}' by '{RequestedBy}'",
                userId, requestedBy);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminController.DeleteUser: User not found with ID '{UserId}'", userId);
                    return NotFound(new { message = "User not found" });
                }

                var username = user.UserName;
                _logger.LogInformation("🗑️ AdminController.DeleteUser: User found '{Username}', checking roles", username);

                // Prevent deleting SuperAdmin users
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin"))
                {
                    _logger.LogWarning("⚠️ AdminController.DeleteUser: Attempt to delete SuperAdmin user '{Username}' by '{RequestedBy}'",
                        username, requestedBy);
                    return BadRequest(new { message = "Cannot delete SuperAdmin users" });
                }

                _logger.LogInformation("🗑️ AdminController.DeleteUser: Deleting user '{Username}' by '{RequestedBy}'",
                    username, requestedBy);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminController.DeleteUser: Failed to delete user '{Username}': {Errors}",
                        username, errors);
                    return BadRequest(new { errors = result.Errors });
                }

                _logger.LogInformation("✅ AdminController.DeleteUser: User '{Username}' deleted successfully by '{RequestedBy}'",
                    username, requestedBy);

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.DeleteUser: Unexpected error deleting user ID '{UserId}'", userId);
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            try
            {
                _logger.LogInformation("🎫 AdminController.GenerateJwtToken: Generating JWT token for '{Username}'", user.UserName);

                var jwtKey = _configuration["EmailAuth:JwtSecretKey"] ?? throw new InvalidOperationException("JWT key not configured");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    _logger.LogInformation("🎫 AdminController.GenerateJwtToken: Added role claim '{Role}' for '{Username}'", role, user.UserName);
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiresAt = DateTime.UtcNow.AddHours(24);
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: expiresAt,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation("✅ AdminController.GenerateJwtToken: JWT token generated successfully for '{Username}', expires at {ExpiresAt}",
                    user.UserName, expiresAt);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminController.GenerateJwtToken: Error generating JWT token for '{Username}'", user.UserName);
                throw;
            }
        }
    }

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
