using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; } // Hashed password

        public bool EmailConfirmed { get; set; } = false; // Defaults to false until verified

        public string? VerificationToken { get; set; } // Token for email verification
        public DateTime? VerificationTokenExpiry { get; set; } // Expiry time for token

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
