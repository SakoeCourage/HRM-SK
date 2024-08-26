﻿
using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffBankDetail
    {
        [Key]
        public Guid Id { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public Guid bankId { get; set; }
        public string accountType { get; set; }
        public string branch { get; set; }
        public string accountNumber { get; set; }
        public virtual Staff staff { get; set; }
        public Boolean isApproved { get; set; } = true;
        public Boolean isAlterable { get; set; } = true;

        public Bank bank { get; set; }

    }
}
