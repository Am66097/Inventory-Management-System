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

                    if (product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for product '{product.ProductName}'. Available: {product.StockQuantity}");
                        ViewBag.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                        return View(sale);
                    }

                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);

                    item.UnitPrice = product.UnitPrice;
                    total += (item.UnitPrice * item.Quantity);

                    sale.SalesItems.Add(item);
                }

                sale.TotalAmount = total;
                sale.SaleDate = DateTime.Now;

                _context.Sales.Add(sale);

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

        // GET: /Sales/Details/1
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales
                .Include(s => s.SalesItems)
                .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(m => m.SaleID == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // GET: /Sales/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            return View(sale);
        }

        // POST: /Sales/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Sale sale)
        {
            if (ModelState.IsValid)
            {
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sale);
        }

        // GET: /Sales/Delete/1
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _context.Sales.FirstOrDefaultAsync(m => m.SaleID == id);
            if (sale == null) return NotFound();

            return View(sale);
        }

        // POST: /Sales/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}