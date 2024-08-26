using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffAppointment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid gradeId { get; set; }
        public Guid staffId { get; set; }
        public Guid? staffSpecialityId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string appointmentType { get; set; } = String.Empty;
        public string staffType { get; set; } = String.Empty;
        public DateOnly? endDate { get; set; }
        public string paymentSource { get; set; } = String.Empty;
        public DateOnly notionalDate { get; set; }
        public DateOnly substantiveDate { get; set; }
        public string step { get; set; }
        public virtual Speciality speciality { get; set; }
        public virtual Grade grade { get; set; }
        public virtual Staff staff { get; set; }
    }
}
