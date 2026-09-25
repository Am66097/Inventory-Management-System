using Microsoft.AspNetCore.Mvc;
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

        // POST: /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));

        }


    }
}
