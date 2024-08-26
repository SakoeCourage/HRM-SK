using HRM_SK.Entities.Staff;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{
    public class Directorate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string directorateName { get; set; }
        public Guid? directorId { get; set; }
        public Guid? depDirectoryId { get; set; }
        public Staff.Staff director { get; set; }
        public Staff.Staff depDirector { get; set; }
        public ICollection<Department> departments { get; set; }
        public ICollection<Unit> units { get; set; }

        [JsonIgnore]
        public virtual ICollection<StaffPosting> staffPostings { get; set; }
    }
}
