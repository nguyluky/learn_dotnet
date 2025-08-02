namespace test_api.Models.Dtos
{
    public class AddPermissionToRoleRequest
    {
        public int RoleId { get; set; }
        public int ActionPermissionId { get; set; }
    }
}
