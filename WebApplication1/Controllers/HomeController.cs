using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // لا نحتاج IConfiguration هنا بعد الآن إذا كان للـ AI فقط
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalSalesRevenue = await _context.Sales.AnyAsync() ? await _context.Sales.SumAsync(s => s.TotalAmount) : 0,
                TotalPurchasesCost = await _context.Purchases.AnyAsync() ? await _context.Purchases.SumAsync(p => p.TotalAmount) : 0,
                LowStockProducts = await _context.Products.Where(p => p.StockQuantity <= p.LowStockThreshold).ToListAsync()
            };

            return View(dashboardData);
        }
    }
}