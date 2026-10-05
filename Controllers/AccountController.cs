using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            string displayName = email.Split('@')[0];
            displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);

            // بدون تحديد Expires تصبح Session Cookie: تُمسح فور إغلاق التاب أو المتصفح
            Response.Cookies.Append("UserName", displayName);
            Response.Cookies.Append("UserEmail", email);

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // جلسة مؤقتة فقط
            Response.Cookies.Append("UserName", fullName);
            Response.Cookies.Append("UserEmail", email);

            return RedirectToAction("Dashboard", "Home");
        }

        public IActionResult Logout()
        {
            // حذف كل الكوكيز تماماً لضمان طلب تسجيل الدخول مجدداً
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserEmail");
            Response.Cookies.Delete("UserAvatar");

            return RedirectToAction("Index", "Home");
        }
    }
}