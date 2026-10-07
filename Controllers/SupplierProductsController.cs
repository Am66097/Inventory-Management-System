using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SupplierProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupplierProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SupplierProducts
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var supplierProducts = _context.SupplierProducts
                .Include(s => s.Supplier)
                .Include(s => s.Product)
                .AsQueryable();

            // Search by Supplier or Product
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                supplierProducts = supplierProducts.Where(s =>
                    s.Supplier.SupplierName.Contains(searchString) ||
                    s.Product.ProductName.Contains(searchString) ||
                    s.SupplierSKU.Contains(searchString));
            }

            return View(await supplierProducts.ToListAsync());
        }


        // GET: SupplierProducts/Create
        public IActionResult Create()
        {
            // 1. استخدام ProductName المتطابق مع الموديل الجديد
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "ProductName");

            // 2. التأكد من اسم حقل المورد (تأكد هل هو SupplierName أم Name في كلاس Supplier)
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");

            // 3. تمرير كائن جديد فارغ لحماية الفيو من أي قراءة للـ Model وهو null
            return View(new SupplierProduct());
        }


        // POST: SupplierProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierProduct supplierProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supplierProduct);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierID"] = new SelectList(
                _context.Suppliers,
                "SupplierId",
                "SupplierName",
                supplierProduct.SupplierID);

            ViewData["ProductID"] = new SelectList(
                _context.Products,
                "ProductID",
                "Name",
                supplierProduct.ProductID);

            return View(supplierProduct);
        }


        // GET: SupplierProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierProduct = await _context.SupplierProducts
                .FindAsync(id);

            if (supplierProduct == null)
            {
                return NotFound();
            }

            ViewData["SupplierID"] = new SelectList(
                _context.Suppliers,
                "SupplierId",
                "SupplierName",
                supplierProduct.SupplierID);

            ViewData["ProductID"] = new SelectList(
                _context.Products,
                "ProductID",
                "Name",
                supplierProduct.ProductID);

            return View(supplierProduct);
        }


        // POST: SupplierProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SupplierProduct supplierProduct)
        {
            if (id != supplierProduct.SupplierProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(supplierProduct);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierID"] = new SelectList(
                _context.Suppliers,
                "SupplierId",
                "SupplierName",
                supplierProduct.SupplierID);

            ViewData["ProductID"] = new SelectList(
                _context.Products,
                "ProductID",
                "Name",
                supplierProduct.ProductID);

            return View(supplierProduct);
        }


        // GET: SupplierProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierProduct = await _context.SupplierProducts
                .Include(s => s.Supplier)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(
                    s => s.SupplierProductID == id);

            if (supplierProduct == null)
            {
                return NotFound();
            }

            return View(supplierProduct);
        }


        // GET: SupplierProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierProduct = await _context.SupplierProducts
                .Include(s => s.Supplier)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(
                    s => s.SupplierProductID == id);

            if (supplierProduct == null)
            {
                return NotFound();
            }

            return View(supplierProduct);
        }


        // POST: SupplierProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplierProduct = await _context.SupplierProducts
                .FindAsync(id);

            if (supplierProduct == null)
            {
                return NotFound();
            }

            _context.SupplierProducts.Remove(supplierProduct);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}