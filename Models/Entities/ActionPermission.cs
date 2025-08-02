namespace test_api.Models.Entities
{
    public class ActionPermission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Path { get; set; } = null!;
        public string Method { get; set; } = null!; // e.g., GET, POST, PUT, DELETE

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        public ICollection<Rule> Rules { get; set; } = null!; // Assuming ActionPermission can be associated with multiple rules
    }

}
