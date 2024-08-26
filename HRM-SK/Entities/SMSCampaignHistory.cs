using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Model.SMS
{
    public class SMSCampaignHistory
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string campaignName { get; set; }
        public Guid? smsTemplateId { get; set; }
        public string message { get; set; }
        public int receipients { get; set; }
        public SMSTemplate smsTemplate { get; set; }
        public ICollection<SMSCampaignReceipient> smsReceipients { get; set; }

    }
}
