using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Purchases/Index
        public IActionResult Index()
        {
            var purchases = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.PurchaseDate)
                .ToList();

            return View(purchases);
        }

        // GET: /Purchases/Details/1
        [HttpGet]
        public IActionResult Details(int id)
        {
            var purchase = _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefault(p => p.PurchaseID == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: /Purchases/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            return View();
        }

        // POST: /Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                if (purchase.PurchaseDate == default)
                {
                    purchase.PurchaseDate = DateTime.Now;
                }

                _context.Purchases.Add(purchase);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", purchase.SupplierID);
            return View(purchase);
        }

        // GET: /Purchases/Edit/1
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var purchase = _context.Purchases.Find(id);
            if (purchase == null)
            {
                return NotFound();
            }

            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", purchase.SupplierID);
            return View(purchase);
        }

        // POST: /Purchases/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                _context.Purchases.Update(purchase);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierID"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", purchase.SupplierID);
            return View(purchase);
        }

        // GET: /Purchases/Delete/1
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var purchase = _context.Purchases
                .Include(p => p.Supplier)
                .FirstOrDefault(p => p.PurchaseID == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: /Purchases/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var purchase = _context.Purchases.Find(id);
            if (purchase == null)
            {
                return NotFound();
            }

            _context.Purchases.Remove(purchase);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
