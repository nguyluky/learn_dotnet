using test_api.Models.Entities;

namespace test_api.Models.Responses
{
    public class GetAllActionsPermissionsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ActionPermission> Data { get; set; }
        public int Total { get; set; }
    }
}
