using HRM_SK.Entities.Staff;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{
    public class Unit
    {
        [Key]
        public Guid Id { get; set; }
        public Guid departmentId { get; set; }
        public Guid? unitHeadId { get; set; }
        public Guid? directorateId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string unitName { get; set; }
        public Department department { get; set; }
        public Directorate directorate { get; set; }
        public Staff.Staff unitHead { get; set; }
        public ICollection<User> users { get; set; }

        [JsonIgnore]
        public virtual ICollection<StaffPosting> staffPostings { get; set; }

    }
}
