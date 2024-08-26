﻿using System.ComponentModel.DataAnnotations;

namespace HRM_SK.Entities.Staff
{
    public class StaffAppointmentHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid gradeId { get; set; }
        public Guid staffId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
        public string appointmentType { get; set; } = String.Empty;
        public Guid? staffSpecialityId { get; set; }

        public DateOnly? endDate { get; set; }
        public string staffType { get; set; } = String.Empty;
        public string paymentSource { get; set; } = String.Empty;
        public DateOnly notionalDate { get; set; }
        public DateOnly substantiveDate { get; set; }
        public string step { get; set; }
        public Grade grade { get; set; }
        public Staff staff { get; set; }
        public Speciality speciality { get; set; }
        public string? scale
        {
            get
            {
                return grade?.scale;
            }
        }
        public virtual string? band
        {
            get
            {
                return grade?.level;
            }
        }
        public virtual string? salary
        {
            get
            {
                return "to be done";
            }
        }
    }
}
