
// order card and payment relastionship graph


namespace test_api.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItem> Items { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Default Status

        // Foreign Key
        public int UserId { get; set; }
        
        // Navigation Property
        public User User { get; set; } = null!; // Assuming User is defined elsewhere in your codebase

    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Foreign Keys
        public int ProductId { get; set; }
        public int OrderId { get; set; }

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Order Order { get; set; } = null!; // Navigation property to the Order
    }
}
