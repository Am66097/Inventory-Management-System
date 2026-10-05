namespace WebApplication1.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalSuppliers { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public decimal TotalPurchasesCost { get; set; }

        // ليستة المنتجات اللي قربت تخلص
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
    }
}