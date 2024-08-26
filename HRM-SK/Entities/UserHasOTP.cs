using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    public class UserHasOTP
    {
        [Key]
        public Guid Id { get; set; }
        public string? email { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string otp { get; set; }
        public Guid userId { get; set; }
        public User user { get; set; }
    }
}
