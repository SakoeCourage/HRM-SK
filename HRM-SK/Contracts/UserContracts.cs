using HRM_SK.Entities.Staff;

namespace HRM_SK.Contracts
{
    public class UserContracts
    {

        public static class UserCredentials
        {


        }

        public class UserLoginResponse
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? emailVerifiedAt { get; set; }
            public DateTime? lastSeen { get; set; }
            public Boolean isAccountActive { get; set; } = true;
            public Boolean hasResetPassword { get; set; } = false;
            public Guid staffId { get; set; }
            public Guid roleId { get; set; }
            public Guid unitId { get; set; }
            public Guid departmentId { get; set; }
            public string email { get; set; }
            public virtual Staff staff { get; set; }
            public string accessToken { get; set; }
            public int unreadCount { get; set; }
            public DateTime expiry { get; set; }
        }


        public class UserDto
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? emailVerifiedAt { get; set; }
            public DateTime? lastSeen { get; set; }
            public Boolean isAccountActive { get; set; } = true;
            public Boolean hasResetPassword { get; set; } = false;
            public Guid? staffId { get; set; } = null;
            public Guid roleId { get; set; }
            public string email { get; set; }
        }

        public class RoleDto
        {
            public Guid Id { get; set; }
            public string name { get; set; }
        }

        public class UserStaffDto
        {
            public string title { get; set; } = string.Empty;
            public string firstName { get; set; } = string.Empty;
            public string lastName { get; set; } = string.Empty;
            public string? otherNames { get; set; } = string.Empty;
        }

        public class UserStaffRoleDto
        {
            public Guid Id { get; set; }
            public DateTime createdAt { get; set; } = DateTime.UtcNow;
            public DateTime updatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? emailVerifiedAt { get; set; }
            public DateTime? lastSeen { get; set; }
            public Boolean isAccountActive { get; set; } = true;
            public Boolean hasResetPassword { get; set; } = false;
            public string email { get; set; }
            public RoleDto role { get; set; }
            public UserStaffDto? staff { get; set; }



        }
    }
}
