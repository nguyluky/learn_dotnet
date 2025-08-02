namespace test_api.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int Rating { get; set; } // Default value for Rating

        // Foreign Keys
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!; // Navigation property to User
        public Product Product { get; set; } = null!; // Navigation property to Product
        public OrderItem OrderItem { get; set; } = null!; // Navigation property to OrderItem
    }
}
