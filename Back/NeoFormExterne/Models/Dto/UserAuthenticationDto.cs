using System.ComponentModel.DataAnnotations;

namespace NeoForm_Externe.Models.Dto
{
    public class UserAuthenticationDto
    {
        public class UserAuthentication
        {
            [Key]
            public string Id { get; set; } = string.Empty;

            [Required]
            public string Guid { get; set; } = string.Empty;

            [Required]
            public string UserId { get; set; } = string.Empty;

            public string? UserEmail { get; set; }

            [Required]
            public string AccessToken { get; set; } = string.Empty;

            public string? IdToken { get; set; }

            public string? RefreshToken { get; set; }

            [Required]
            public DateTime ExpiresAt { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; }

            [Required]
            public bool IsActive { get; set; }
        }
    }
}
