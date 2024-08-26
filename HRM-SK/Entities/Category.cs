using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{

    [Index(nameof(categoryName), IsUnique = true)]
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string categoryName { get; set; } = String.Empty;
        public ICollection<Grade> grades { get; set; }
        public ICollection<Speciality> specialities { get; set; }
    }
}
