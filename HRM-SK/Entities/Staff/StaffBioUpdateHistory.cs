using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffBioUpdateHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string title { get; set; } = string.Empty;
        public string nationality { get; set; } = string.Empty;
        public string? GPSAddress { get; set; } = null;
        public string staffIdentificationNumber { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string? otherNames { get; set; } = string.Empty;
        public string? ECOWASCardNumber { get; set; } = null;
        public DateOnly? dateOfBirth { get; set; }
        public string? phone { get; set; } = null;
        public string gender { get; set; } = string.Empty;
        public string? SNNITNumber { get; set; } = null;
        public string email { get; set; } = string.Empty;
        public string disability { get; set; } = string.Empty;
        public Boolean isApproved { get; set; } = false;
        public Staff staff { get; set; }
    }
}
