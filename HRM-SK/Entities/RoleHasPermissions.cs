namespace HRM_SK.Entities
{
    public class RoleHasPermissions
    {
        public Guid roleId { get; set; }
        public Guid permissionId { get; set; }

        public Role role { get; set; }
        public Permission permission { get; set; }
    }
}
