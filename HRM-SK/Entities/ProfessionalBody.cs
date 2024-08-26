using HRM_SK.Entities.Staff;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    public class ProfessionalBody
    {
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public Collection<StaffProfessionalLincense> staffProfessionalLincense { get; set; }
    }
}
