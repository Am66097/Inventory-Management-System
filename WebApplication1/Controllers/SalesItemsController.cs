using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class SalesItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SalesItems
        public async Task<IActionResult> Index()
        {
            var salesItems = await _context.SalesItems
                .Include(s => s.Sale)
                .Include(s => s.Product)
                .ToListAsync();

            return View(salesItems);
        }

        // GET: SalesItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var salesItem = await _context.SalesItems
                .Include(s => s.Sale)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SaleItemID == id);

            if (salesItem == null) return NotFound();

            return View(salesItem);
        }

        // GET: SalesItems/Create
        public IActionResult Create()
        {
            ViewData["SaleID"] = new SelectList(_context.Sales, "SaleID", "SaleID");
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name");
            return View();
        }

        // POST: SalesItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleItem salesItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salesItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SaleID"] = new SelectList(_context.Sales, "SaleID", "SaleID", salesItem.SaleID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", salesItem.ProductID);
            return View(salesItem);
        }

        // GET: SalesItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var salesItem = await _context.SalesItems.FindAsync(id);
            if (salesItem == null) return NotFound();

            ViewData["SaleID"] = new SelectList(_context.Sales, "SaleID", "SaleID", salesItem.SaleID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", salesItem.ProductID);
            return View(salesItem);
        }

        // POST: SalesItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SaleItem salesItem)
        {
            if (id != salesItem.SaleItemID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(salesItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SaleID"] = new SelectList(_context.Sales, "SaleID", "SaleID", salesItem.SaleID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", salesItem.ProductID);
            return View(salesItem);
        }

        // GET: SalesItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var salesItem = await _context.SalesItems
                .Include(s => s.Sale)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.SaleItemID == id);

            if (salesItem == null) return NotFound();

            return View(salesItem);
        }

        // POST: SalesItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesItem = await _context.SalesItems.FindAsync(id);
            if (salesItem != null)
            {
                _context.SalesItems.Remove(salesItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}