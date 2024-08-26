﻿using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities
{
    public class UnitHead
    {
        [Key]
        public Guid Id { get; set; }
        public Guid unitId { get; set; }
        public string position { get; set; } = String.Empty;
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public Unit unit { get; set; }

    }
}
