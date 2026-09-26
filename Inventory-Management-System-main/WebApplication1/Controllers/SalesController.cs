using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // أو النيمسبيس الخاص بالـ DbContext لديك
using WebApplication1.Models;


namespace WebApplication1.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض قائمة جميع المبيعات
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            return View(sales);
        }

        // عرض شاشة إنشاء فاتورة جديدة (GET)
        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
            return View();
        }

        // حفظ الفاتورة وتطبيق اللوجيك الأهم (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale, List<SaleItem> items)
        {
            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "Please add at least one item to the sale.");
                ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(sale);
            }

            decimal total = 0;

            // استخدام Transaction لضمان سلامة قاعدة البيانات
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductID);

                    if (product == null)
                    {
                        ModelState.AddModelError("", $"Product with ID {item.ProductID} not found.");
                        ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                        return View(sale);
                    }

                    // التحقق من توافر الكمية في الستوك
                    if (product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for product '{product.ProductName}'. Available: {product.StockQuantity}");
                        ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                        return View(sale);
                    }

                    // 1. اللوجيك الأهم: الخصم التلقائي من الستوك
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);

                    // 2. حساب السعر وتجميع الإجمالي أوتوماتيك
                    item.UnitPrice = product.UnitPrice;
                    total += (item.UnitPrice * item.Quantity);

                    // إضافة العنصر للفاتورة
                    sale.SalesItems.Add(item);
                }

                // 3. تعيين إجمالي الفاتورة وتاريخ الشراء
                sale.TotalAmount = total;
                sale.SaleDate = DateTime.Now;

                _context.Sales.Add(sale);

                // حفظ التغييرات ودمج العمليات
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "An error occurred while saving the sale: " + ex.Message);
                ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(sale);
            }
        }
    }
}