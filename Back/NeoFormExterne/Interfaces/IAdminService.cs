using Microsoft.AspNetCore.Identity;
using NeoForm_Externe.Models;

namespace NeoForm_Externe.Interfaces
{
    public interface IAdminService
    {
        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        Task<AdminLoginResult> LoginAsync(string username, string password);

        /// <summary>
        /// Changes the password for a user
        /// </summary>
        Task<AdminOperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Creates a new admin user
        /// </summary>
        Task<AdminCreateUserResult> CreateAdminAsync(string username, string email, string password, string? fullName, string createdBy);

        /// <summary>
        /// Gets all users with their roles
        /// </summary>
        Task<List<AdminUserInfo>> GetAllUsersAsync();

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        Task<AdminOperationResult> DeleteUserAsync(string userId);
    }

    // Result models for AdminService
    public class AdminLoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public IList<string>? Roles { get; set; }
        public string? FullName { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsLockedOut { get; set; }
    }

    public class AdminOperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<IdentityError>? Errors { get; set; }
    }

    public class AdminCreateUserResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public IEnumerable<IdentityError>? Errors { get; set; }
    }

    public class AdminUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
