using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeoForm_Externe.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AdminLoginResult> LoginAsync(string username, string password)
        {
            _logger.LogInformation("🔐 AdminService.LoginAsync: Login attempt for username '{Username}'", username);

            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminService.LoginAsync: User not found: '{Username}'", username);
                    return new AdminLoginResult
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                _logger.LogInformation("🔐 AdminService.LoginAsync: User found, checking password for '{Username}'", username);
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("⚠️ AdminService.LoginAsync: Account locked out for '{Username}'", username);
                        return new AdminLoginResult
                        {
                            Success = false,
                            Message = "Account is locked. Please try again later.",
                            IsLockedOut = true
                        };
                    }

                    _logger.LogWarning("⚠️ AdminService.LoginAsync: Invalid password for '{Username}'", username);
                    return new AdminLoginResult
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("🔐 AdminService.LoginAsync: Password verified for '{Username}', Roles: {Roles}",
                    username, string.Join(", ", roles));

                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation("✅ AdminService.LoginAsync: Login successful for '{Username}', MustChangePassword: {MustChange}",
                    username, user.MustChangePassword);

                return new AdminLoginResult
                {
                    Success = true,
                    Token = token,
                    Username = user.UserName,
                    Roles = roles,
                    FullName = user.FullName,
                    MustChangePassword = user.MustChangePassword
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.LoginAsync: Unexpected error during login for '{Username}'", username);
                throw;
            }
        }

        public async Task<AdminOperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            _logger.LogInformation("🔑 AdminService.ChangePasswordAsync: Password change request for user ID '{UserId}'", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminService.ChangePasswordAsync: User not found for ID '{UserId}'", userId);
                    return new AdminOperationResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                _logger.LogInformation("🔑 AdminService.ChangePasswordAsync: Attempting to change password for '{Username}'", user.UserName);
                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminService.ChangePasswordAsync: Password change failed for '{Username}': {Errors}",
                        user.UserName, errors);
                    return new AdminOperationResult
                    {
                        Success = false,
                        Message = "Password change failed",
                        Errors = result.Errors
                    };
                }

                // Remove the must change password flag
                var wasForcedChange = user.MustChangePassword;
                user.MustChangePassword = false;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("✅ AdminService.ChangePasswordAsync: Password changed successfully for '{Username}', ForcedChange: {WasForced}",
                    user.UserName, wasForcedChange);

                return new AdminOperationResult
                {
                    Success = true,
                    Message = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.ChangePasswordAsync: Unexpected error for user ID '{UserId}'", userId);
                throw;
            }
        }

        public async Task<AdminCreateUserResult> CreateAdminAsync(string username, string email, string password, string? fullName, string createdBy)
        {
            _logger.LogInformation("👤 AdminService.CreateAdminAsync: Request to create admin user '{Username}' by '{CreatedBy}'",
                username, createdBy);

            try
            {
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    _logger.LogWarning("⚠️ AdminService.CreateAdminAsync: Username '{Username}' already exists", username);
                    return new AdminCreateUserResult
                    {
                        Success = false,
                        Message = "Username already exists"
                    };
                }

                _logger.LogInformation("👤 AdminService.CreateAdminAsync: Creating new admin user '{Username}'", username);

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = fullName,
                    CreatedBy = createdBy
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminService.CreateAdminAsync: Failed to create user '{Username}': {Errors}",
                        username, errors);
                    return new AdminCreateUserResult
                    {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = result.Errors
                    };
                }

                _logger.LogInformation("👤 AdminService.CreateAdminAsync: User '{Username}' created, assigning Admin role", username);

                // Ensure Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    _logger.LogInformation("👤 AdminService.CreateAdminAsync: Creating Admin role");
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Assign Admin role
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("✅ AdminService.CreateAdminAsync: Admin user '{Username}' created successfully by '{CreatedBy}'",
                        username, createdBy);
                }
                else
                {
                    _logger.LogWarning("⚠️ AdminService.CreateAdminAsync: User created but role assignment failed for '{Username}'",
                        username);
                }

                return new AdminCreateUserResult
                {
                    Success = true,
                    Message = "Admin user created successfully",
                    Username = user.UserName,
                    Role = "Admin"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.CreateAdminAsync: Unexpected error creating admin user '{Username}'", username);
                throw;
            }
        }

        public async Task<List<AdminUserInfo>> GetAllUsersAsync()
        {
            _logger.LogInformation("📋 AdminService.GetAllUsersAsync: Retrieving all users");

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
                var usersWithRoles = new List<AdminUserInfo>();
                foreach (var user in users)
                {
                    var appUser = await _userManager.FindByIdAsync(user.Id);
                    var roles = await _userManager.GetRolesAsync(appUser!);
                    usersWithRoles.Add(new AdminUserInfo
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        CreatedAt = user.CreatedAt,
                        CreatedBy = user.CreatedBy,
                        Roles = roles
                    });
                }

                _logger.LogInformation("✅ AdminService.GetAllUsersAsync: Retrieved {Count} users", users.Count);
                return usersWithRoles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.GetAllUsersAsync: Error retrieving users");
                throw;
            }
        }

        public async Task<AdminOperationResult> DeleteUserAsync(string userId)
        {
            _logger.LogInformation("🗑️ AdminService.DeleteUserAsync: Delete request for user ID '{UserId}'", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ AdminService.DeleteUserAsync: User not found with ID '{UserId}'", userId);
                    return new AdminOperationResult
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var username = user.UserName;
                _logger.LogInformation("🗑️ AdminService.DeleteUserAsync: User found '{Username}', checking roles", username);

                // Prevent deleting SuperAdmin users
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("SuperAdmin"))
                {
                    _logger.LogWarning("⚠️ AdminService.DeleteUserAsync: Attempt to delete SuperAdmin user '{Username}'", username);
                    return new AdminOperationResult
                    {
                        Success = false,
                        Message = "Cannot delete SuperAdmin users"
                    };
                }

                _logger.LogInformation("🗑️ AdminService.DeleteUserAsync: Deleting user '{Username}'", username);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("⚠️ AdminService.DeleteUserAsync: Failed to delete user '{Username}': {Errors}",
                        username, errors);
                    return new AdminOperationResult
                    {
                        Success = false,
                        Message = "Failed to delete user",
                        Errors = result.Errors
                    };
                }

                _logger.LogInformation("✅ AdminService.DeleteUserAsync: User '{Username}' deleted successfully", username);

                return new AdminOperationResult
                {
                    Success = true,
                    Message = "User deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.DeleteUserAsync: Unexpected error deleting user ID '{UserId}'", userId);
                throw;
            }
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            try
            {
                _logger.LogInformation("🎫 AdminService.GenerateJwtToken: Generating JWT token for '{Username}'", user.UserName);

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
                    _logger.LogInformation("🎫 AdminService.GenerateJwtToken: Added role claim '{Role}' for '{Username}'", role, user.UserName);
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
                _logger.LogInformation("✅ AdminService.GenerateJwtToken: JWT token generated successfully for '{Username}', expires at {ExpiresAt}",
                    user.UserName, expiresAt);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ AdminService.GenerateJwtToken: Error generating JWT token for '{Username}'", user.UserName);
                throw;
            }
        }
    }
}
