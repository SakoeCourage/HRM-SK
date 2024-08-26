using HRM_SK.Entities.Staff;

namespace HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio
{
    public class UnitDto
    {
        public Guid id { get; set; }
        public string unitName { get; set; }
    }
    public class DepartmentDto
    {
        public Guid id { get; set; }
        public string departmentName { get; set; }
    }

    public class DirectorateDto
    {
        public Guid id { get; set; }
        public string directorateName { get; set; }
    }
    public class SpecialityDto
    {
        public Guid id { get; set; }
        public Guid categoryId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string specialityName { get; set; } = String.Empty;
    }

    public class CategoryDto
    {
        public Guid id { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
        public string categoryName { get; set; } = String.Empty;
    }

    public class StaffPostingDto
    {
        public Guid id { get; set; }
        public Guid staffId { get; set; }
        public Guid? directorateId { get; set; }
        public Guid? departmentId { get; set; }
        public Guid? unitId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public DateOnly postingDate { get; set; }

    }

    public class StaffDto
    {
        public Guid id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public string title { get; set; }
        public string gpsAddress { get; set; }
        public string staffIdentificationNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string? otherNames { get; set; }
        public DateOnly? dateOfBirth { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public string SNNITNumber { get; set; }
        public string email { get; set; }
        public string disability { get; set; }
        public string? passportPicture { get; set; }
        public string ECOWASCardNumber { get; set; }
        public string status { get; set; }
        public bool isApproved { get; set; }
        public bool isAlterable { get; set; }
        public UnitDto? unit { get; set; }
        public DepartmentDto? department { get; set; }
        public DirectorateDto? directorate { get; set; }
        public StaffPostingDto? staffPosting { get; set; }
        public SpecialityDto? speciality { get; set; }
        public CategoryDto? category { get; set; }
        public StaffAppointment? currentAppointment { get; set; }
        public StaffAppointment? firstAppointment { get; set; }
    }
}
