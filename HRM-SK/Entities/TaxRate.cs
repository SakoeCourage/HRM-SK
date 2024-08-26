using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    [Index(nameof(year), IsUnique = true)]

    public class TaxRate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public DateOnly year { get; set; }
        public ICollection<TaxRateDetail> taxRateDetails { get; set; }
    }
}
