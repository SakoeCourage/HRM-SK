namespace HRM_SK.Entities
{
    public class UserHasRole
    {
        public Guid userId { get; set; }
        public Guid roleId { get; set; }
        public User user { get; set; }
        public Role role { get; set; }
    }
}
