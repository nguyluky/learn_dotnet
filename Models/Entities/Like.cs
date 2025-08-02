namespace test_api.Models.Entities
{
    public class LikeProduct
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int UserId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!; // Navigation property to User
        public Product Product { get; set; } = null!; // Navigation property to Product
    }
}
