using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffProfessionalLincenseUpdateHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid professionalBodyId { get; set; }
        public Guid staffId { get; set; }
        public string pin { get; set; }
        public DateOnly issuedDate { get; set; }
        public DateOnly expiryDate { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public ProfessionalBody ProfessionalBody { get; set; }
        public Staff staff { get; set; }
        public Boolean isApproved { get; set; }
    }
}
