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
        public async Task<IActionResult> Index()
        {
            var supplierProducts = await _context.SupplierProducts
                .Include(s => s.Supplier)
                .Include(s => s.Product)
                .ToListAsync();

            return View(supplierProducts);
        }

        // GET: SupplierProducts/Create
        public IActionResult Create()
        {
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierID", "SupplierName");
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name");
            return View();
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
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierID", "SupplierName", supplierProduct.SupplierID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", supplierProduct.ProductID);
            return View(supplierProduct);
        }
    }
}