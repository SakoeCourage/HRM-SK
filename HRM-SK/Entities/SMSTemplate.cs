using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Model.SMS
{
    public class SMSTemplate
    {
        [Key]
        public Guid Id { get; set; }
        public string name { get; set; }
        public string message { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string? description { get; set; }
        public Boolean readOnly { get; set; }
        [JsonIgnore]
        public ICollection<SMSCampaignHistory> smsHistory { get; set; }
    }
}
