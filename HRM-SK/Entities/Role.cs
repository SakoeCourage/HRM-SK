using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{
    [Index(nameof(name), IsUnique = true)]
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Permission> permissions { get; set; }

        [JsonIgnore]
        public ICollection<User> users { get; set; }
    }
}
