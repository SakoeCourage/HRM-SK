using HRM_SK.Entities.Staff;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }
        public Guid directorateId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string departmentName { get; set; }
        public Guid? headOfDepartmentId { get; set; }
        public Guid? depHeadOfDepartmentId { get; set; }
        public Staff.Staff headOfDepartment { get; set; }
        public Staff.Staff depHeadOfDepartment { get; set; }
        public Directorate directorate { get; set; }
        public ICollection<Unit> units { set; get; }

        [JsonIgnore]
        public virtual ICollection<StaffPosting> staffPostings { get; set; }
        public ICollection<User> users { get; set; }


    }
}
