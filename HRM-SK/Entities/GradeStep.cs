using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{
    public class GradeStep
    {
        [Key]
        public Guid Id { get; set; }
        public Guid gradeId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public int stepIndex { get; set; }
        public Double salary { get; set; }
        public Double marketPreBaseSalary { get; set; }
        [JsonIgnore]
        public Grade grade { get; set; }
    }
}
