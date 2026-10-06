using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependancy Injection
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Categories
        public IActionResult Index()
        {
            var catrigories = _context.Categories.ToList();
            return View(catrigories);
        }

        // GET: /Categories/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Categories/Create
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index)); // يرجعك لصفحة الجدول بعد الحفظ
            }
            return View(category); // لو في خطأ في الإدخال، يرجعه لنفس الصفحة بالداتا القديمة
        }

        // GET: /Categories/Edit/1
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: /Categories/Edit/1
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Get : /Categories/Details/1
        [HttpGet]
        public IActionResult Details(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // GET: /Categories/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasProducts = await _context.Products.AnyAsync(p => p.CategoryID == id);

            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Cannot delete this category because it contains active products. Please reassign or delete the products first.";
                return RedirectToAction(nameof(Index));
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
