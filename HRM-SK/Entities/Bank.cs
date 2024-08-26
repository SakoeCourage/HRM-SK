﻿using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    public class Bank
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string bankName { get; set; } = String.Empty;
        public virtual ICollection<Staff.StaffBankDetail> staffbankDetails { get; set; }
    }
}
