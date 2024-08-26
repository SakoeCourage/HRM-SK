using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffChildrenDetail
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string childName { get; set; } = string.Empty;
        public DateOnly dateOfBirth { get; set; }
        public string gender { get; set; } = string.Empty;
        public Staff staff { get; set; }
        public Boolean isApproved { get; set; }
        public Boolean isAlterable { get; set; } = false;


    }
}
