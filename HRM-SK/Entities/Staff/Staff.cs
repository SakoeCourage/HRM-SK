using HRM_SK.Contracts;
using HRM_SK.Entities.HRMActivities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities.Staff
{
    [Index(nameof(staffIdentificationNumber), IsUnique = true)]
    [Index(nameof(email), IsUnique = true)]
    public class Staff
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? lastSeen { get; set; }
        public string title { get; set; } = string.Empty;
        public string GPSAddress { get; set; } = string.Empty;
        public string nationality { get; set; } = string.Empty;
        public string staffIdentificationNumber { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string? otherNames { get; set; } = string.Empty;
        public DateOnly? dateOfBirth { get; set; }
        public string? phone { get; set; } = null;
        public string gender { get; set; } = string.Empty;
        public string? SNNITNumber { get; set; } = null;
        public string email { get; set; } = string.Empty;
        public string disability { get; set; } = string.Empty;
        public string? passportPicture { get; set; } = string.Empty;
        public string? ECOWASCardNumber { get; set; } = null;
        public string status { get; set; } = StaffStatusTypes.active;
        public StaffPosting? staffPosting { get; set; }
        public Boolean isApproved { get; set; } = false;
        public Boolean isAlterable { get; set; } = false;
        [JsonIgnore]
        public User? user { get; set; }
        public Unit unit { get; set; }
        public StaffBankDetail bankDetail { get; set; }
        public StaffFamilyDetail familyDetail { get; set; }
        public StaffProfessionalLincense professionalLincense { get; set; }
        public ICollection<StaffChildrenDetail> staffChildren { get; set; }
        public StaffAccomodationDetail staffAccomodation { get; set; }
        public StaffAppointment currentAppointment { get; set; }
        public ICollection<StaffAppointmentHistory> appointmentHistory { get; set; }
        public ICollection<StaffBioUpdateHistory> bioUpdateHistory { get; set; }
        public Seperation separation { get; set; }
        public ICollection<StaffPostingHistory> transferHistory { get; set; }
    }
}
