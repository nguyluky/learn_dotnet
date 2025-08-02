namespace test_api.Models.Dtos
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Banner { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateStoreDto
    {
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Banner { get; set; } = null!;
    }

    public class UpdateStoreDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public string? Banner { get; set; }
    }

    public class SellerStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public List<MonthlySellerRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<TopSellerProductDto> TopProducts { get; set; } = new();
    }

    public class MonthlySellerRevenueDto
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopSellerProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
