using Microsoft.AspNetCore.Identity;

namespace NeoForm_Externe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public bool MustChangePassword { get; set; } = false; // Force password change on first login
    }
}
