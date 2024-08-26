﻿using HRM_SK.Entities.Staff;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRM_SK.Entities
{

    [Index(nameof(gradeName), IsUnique = true)]
    public class Grade
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public Guid categoryId { get; set; } = Guid.Empty;
        public string gradeName { get; set; } = String.Empty;
        public string level { get; set; }
        public string scale { get; set; }
        public Double marketPremium { get; set; }
        public int minimunStep { get; set; }
        public int maximumStep { get; set; }
        public Category category { get; set; }
        public ICollection<GradeStep> steps { get; set; }
        [JsonIgnore]
        public ICollection<StaffAppointment> appointments { get; set; }


    }
}
