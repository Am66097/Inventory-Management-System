using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
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

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            return View(sales);
        }

        // GET: Sales/Create
        public IActionResult Create()
        {
            ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sale sale, List<SaleItem> items)
        {
            // 1. التحقق من الـ ModelState (خطوة أساسية لا يمكن تجاهلها)
            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(sale);
            }

            if (items == null || !items.Any())
            {
                ModelState.AddModelError("", "Please add at least one item to the sale.");
                ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(sale);
            }

            // 2. تهيئة الـ List لتجنب خطأ NullReferenceException عند عمل Add
            sale.SalesItems = new List<SaleItem>();
            decimal total = 0;

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

                    // الخصم التلقائي من الستوك
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);

                    // حساب السعر وتجميع الإجمالي أوتوماتيك
                    item.UnitPrice = product.UnitPrice;
                    total += (item.UnitPrice * item.Quantity);

                    // إضافة العنصر للفاتورة بأمان
                    sale.SalesItems.Add(item);
                }

                // تعيين إجمالي الفاتورة وتاريخ الشراء
                sale.TotalAmount = total;
                sale.SaleDate = DateTime.Now;

                _context.Sales.Add(sale);

                // 3. حفظ التغييرات 
                // (دالة SaveChangesAsync في EF Core تقوم بعمل Transaction تلقائياً لجميع العمليات، فلا حاجة لكتابتها يدوياً)
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the sale: " + ex.Message);
                ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(sale);
            }
        }
    }
}