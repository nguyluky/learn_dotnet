

namespace test_api.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;
        public string ShortDescription { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;

        // Foreign Keys
        public int CategoryId { get; set; }
        public int StoreId { get; set; }

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public Store Store { get; set; } = null!;

        public bool IsAvailable { get; set; } = true; // Default value for IsAvailable

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
