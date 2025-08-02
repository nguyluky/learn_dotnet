namespace test_api.Models.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        
        // Foreign Key
        public int OwnerId { get; set; }
        
        // Navigation Property
        public User Owner { get; set; } = null!;
        public string avtar { get; set; } = null!; // URL or path to the store's avatar image
        public string banner { get; set; } = null!; // URL or path to the store's banner image

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; } = null!; // Navigation property to Products
    }
}
