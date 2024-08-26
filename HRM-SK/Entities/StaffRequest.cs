using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    [Index(nameof(RequestDetailPolymorphicId), IsUnique = false)]
    public class StaffRequest
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? requestFromStaffId { get; set; }
        public Guid? requestAssignedStaffId { get; set; }
        public Guid RequestDetailPolymorphicId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string requestType { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public Staff.Staff requestFromStaff { get; set; }
        public Staff.Staff requestAssignedStaff { get; set; }

    }
}
