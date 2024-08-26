using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffAccomodationDetail
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public Guid staffId { get; set; }
        public string source { get; set; } = String.Empty;
        public string gpsAddress { get; set; } = String.Empty;
        public string accomodationType { get; set; } = String.Empty;
        public DateOnly? allocationDate { get; set; }
        public string? flatNumber { get; set; } = String.Empty;
        public Boolean isApproved { get; set; } = true;
        public Boolean isAlterable { get; set; } = true;
        public Staff staff { get; set; }

    }
}
