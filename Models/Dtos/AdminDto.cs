namespace test_api.Models.Dtos
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
