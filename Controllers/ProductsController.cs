
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        // عرض المنتجات + البحث + فلترة Category + فلترة Status + Pagination
        public async Task<IActionResult> Index(
            string searchString,
            int? categoryId,
            string status,
            int pageNumber = 1,
            int pageSize = 5)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentStatus"] = status;

            // جلب المنتجات مع الـ Category
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // =========================
            // Search
            // =========================

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchString) ||
                    p.Description.Contains(searchString));
            }

            // =========================
            // Filter by Category
            // =========================

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryID == categoryId.Value);
            }

            // =========================
            // Filter by Stock Status
            // =========================

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "InStock")
                {
                    query = query.Where(p =>
                        p.StockQuantity > p.LowStockThreshold);
                }
                else if (status == "LowStock")
                {
                    query = query.Where(p =>
                        p.StockQuantity > 0 &&
                        p.StockQuantity <= p.LowStockThreshold);
                }
                else if (status == "OutOfStock")
                {
                    query = query.Where(p =>
                        p.StockQuantity == 0);
                }
            }

            // =========================
            // Pagination
            // =========================

            int totalItems = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                (double)totalItems / pageSize);

            // منع الوصول لصفحة غير موجودة
            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var products = await query
                .OrderByDescending(p => p.ProductID)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // =========================
            // Data for View
            // =========================

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            // Categories used by the filter
            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync(),
                "CategoryId",
                "CategoryName",
                categoryId);

            return View(products);
        }


        // GET: Products/Details/5
        // صفحة تفاصيل المنتج
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // GET: Products/Create
        // صفحة إنشاء منتج جديد
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName");

            return View();
        }


        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryID"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }


        // GET: Products/Edit/5
        // صفحة تعديل المنتج
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryID"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }


        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(product);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryID"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                product.CategoryID);

            return View(product);
        }


        // GET: Products/Delete/5
        // صفحة تأكيد الحذف
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

