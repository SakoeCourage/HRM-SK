using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffPosting
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public Guid? directorateId { get; set; }
        public Guid? departmentId { get; set; }
        public Guid? unitId { get; set; }
        public DateOnly postingDate { get; set; }
        public Staff Staff { get; set; }
        public Unit unit { get; set; }
        public Department department { get; set; }
        public Directorate directorate { get; set; }
        public Boolean isAlterable { get; set; } = false;

    }
}
