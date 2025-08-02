namespace test_api.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string Password { get; set; } = null!;

        public int RuleId { get; set; } // Foreign key to the Rule entity
        public Rule Rule { get; set; } = null!; // Assuming Rule is a string identifier for the user's role

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;


        public ICollection<Order> Orders { get; set; } = null!; // Navigation property to Orders

    }
}
