using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PurchaseItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        public PurchaseItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PurchaseItems
        public IActionResult Index()
        {
            var purchaseItems = _context.PurchaseItems
                .Include(pi => pi.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .Include(pi => pi.Product)
                .ToList();

            return View(purchaseItems);
        }

        // GET: /PurchaseItems/Details/1
        [HttpGet]
        public IActionResult Details(int id)
        {
            var purchaseItem = _context.PurchaseItems
                .Include(pi => pi.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .Include(pi => pi.Product)
                .FirstOrDefault(m => m.PurchaseItemID == id);

            if (purchaseItem == null)
            {
                return NotFound();
            }

            return View(purchaseItem);
        }

        // GET: /PurchaseItems/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["PurchaseID"] = new SelectList(_context.Purchases, "PurchaseID", "PurchaseID");
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name");
            return View();
        }

        // POST: /PurchaseItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PurchaseItem purchaseItem)
        {
            if (ModelState.IsValid)
            {
                // زيادة كمية المنتج في المخزن عند الشراء
                var product = _context.Products.Find(purchaseItem.ProductID);
                if (product != null)
                {
                    product.StockQuantity += purchaseItem.Quantity;
                    _context.Products.Update(product);
                }

                // تحديث إجمالي الفاتورة
                var purchase = _context.Purchases.Find(purchaseItem.PurchaseID);
                if (purchase != null)
                {
                    purchase.TotalAmount += (purchaseItem.UnitCost * purchaseItem.Quantity);
                    _context.Purchases.Update(purchase);
                }

                _context.PurchaseItems.Add(purchaseItem);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PurchaseID"] = new SelectList(_context.Purchases, "PurchaseID", "PurchaseID", purchaseItem.PurchaseID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", purchaseItem.ProductID);
            return View(purchaseItem);
        }

        // GET: /PurchaseItems/Edit/1
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var purchaseItem = _context.PurchaseItems.Find(id);
            if (purchaseItem == null)
            {
                return NotFound();
            }

            ViewData["PurchaseID"] = new SelectList(_context.Purchases, "PurchaseID", "PurchaseID", purchaseItem.PurchaseID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", purchaseItem.ProductID);
            return View(purchaseItem);
        }

        // POST: /PurchaseItems/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PurchaseItem purchaseItem)
        {
            if (ModelState.IsValid)
            {
                _context.PurchaseItems.Update(purchaseItem);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PurchaseID"] = new SelectList(_context.Purchases, "PurchaseID", "PurchaseID", purchaseItem.PurchaseID);
            ViewData["ProductID"] = new SelectList(_context.Products, "ProductID", "Name", purchaseItem.ProductID);
            return View(purchaseItem);
        }

        // GET: /PurchaseItems/Delete/1
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var purchaseItem = _context.PurchaseItems
                .Include(pi => pi.Purchase)
                    .ThenInclude(p => p!.Supplier)
                .Include(pi => pi.Product)
                .FirstOrDefault(m => m.PurchaseItemID == id);

            if (purchaseItem == null)
            {
                return NotFound();
            }

            return View(purchaseItem);
        }

        // POST: /PurchaseItems/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var purchaseItem = _context.PurchaseItems.Find(id);
            if (purchaseItem == null)
            {
                return NotFound();
            }

            // تقليل كمية المنتج في المخزن لو اتحذف بند الشراء
            var product = _context.Products.Find(purchaseItem.ProductID);
            if (product != null)
            {
                product.StockQuantity -= purchaseItem.Quantity;
                if (product.StockQuantity < 0) product.StockQuantity = 0;
                _context.Products.Update(product);
            }

            // خصم من إجمالي الفاتورة
            var purchase = _context.Purchases.Find(purchaseItem.PurchaseID);
            if (purchase != null)
            {
                purchase.TotalAmount -= (purchaseItem.UnitCost * purchaseItem.Quantity);
                if (purchase.TotalAmount < 0) purchase.TotalAmount = 0;
                _context.Purchases.Update(purchase);
            }

            _context.PurchaseItems.Remove(purchaseItem);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
