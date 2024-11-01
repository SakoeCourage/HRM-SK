﻿using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.HRMActivities
{
    public class Seperation
    {
        [Key]
        public Guid Id { get; set; }
        public Guid StaffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; }
        public DateOnly DateOfSeparation { get; set; }
        public string? comment { get; set; }
        public virtual Staff.Staff Staff { get; set; }

    }
}
