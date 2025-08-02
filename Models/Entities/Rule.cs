namespace test_api.Models.Entities
{
    public class Rule
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsAdmin { get; set; } = false; // Indicates if this rule is for admin users
        public bool IsDefault { get; set; } = false; // Indicates if this rule is the default rule

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        public ICollection<ActionPermission> ActionPermissions { get; set; } = null!;
        public ICollection<User> Users { get; set; } = null!;
    }
}
