using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. الصفحة الرئيسية (Landing Page)
        public IActionResult Index()
        {
            return View();
        }

        // 2. لوحة التحكم (Dashboard)
        // 2. لوحة التحكم المطورة (Dashboard)
        public async Task<IActionResult> Dashboard()
        {
            var lowStock = await _context.Products.Where(p => p.StockQuantity <= p.LowStockThreshold).ToListAsync();

            var dashboardData = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalSalesRevenue = await _context.Sales.AnyAsync() ? await _context.Sales.SumAsync(s => s.TotalAmount) : 0,
                TotalPurchasesCost = await _context.Purchases.AnyAsync() ? await _context.Purchases.SumAsync(p => p.TotalAmount) : 0,
                LowStockProducts = lowStock
            };

            // جلب أحدث 4 فواتير بيع كنشاطات حديثة
            ViewBag.RecentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(4)
                .ToListAsync();

            return View(dashboardData);
        }

        // 3. التحليلات (Analytics)
        public async Task<IActionResult> Analytics()
        {
            ViewBag.TotalSales = await _context.Sales.AnyAsync() ? await _context.Sales.SumAsync(s => s.TotalAmount) : 0;
            ViewBag.TotalPurchases = await _context.Purchases.AnyAsync() ? await _context.Purchases.SumAsync(p => p.TotalAmount) : 0;
            ViewBag.ProductsCount = await _context.Products.CountAsync();
            return View();
        }

        // 4. التقارير (Reports)
        public async Task<IActionResult> Reports()
        {
            var lowStock = await _context.Products.Where(p => p.StockQuantity <= p.LowStockThreshold).ToListAsync();
            return View(lowStock);
        }

        // 5. صفحة الإعدادات وتعديل البيانات (GET)
        [HttpGet]
        public IActionResult Settings()
        {
            ViewBag.UserName = Request.Cookies["UserName"] ?? "Admin";
            ViewBag.UserEmail = Request.Cookies["UserEmail"] ?? "admin@stockora.com";
            ViewBag.UserAvatar = Request.Cookies["UserAvatar"] ?? "";
            return View();
        }

        // حفظ تعديلات الملف الشخصي (الاسم، الجيميل، الباسورد، الصورة)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string? newPassword, string? confirmPassword, IFormFile? profileImage)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                TempData["ProfileError"] = "Name and Email cannot be empty.";
                return RedirectToAction("Settings");
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["ProfileError"] = "Passwords do not match!";
                    return RedirectToAction("Settings");
                }
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profileImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                string avatarUrl = "/uploads/" + uniqueFileName;
                Response.Cookies.Append("UserAvatar", avatarUrl, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
            }

            Response.Cookies.Append("UserName", fullName, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
            Response.Cookies.Append("UserEmail", email, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });

            TempData["ProfileSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Settings");
        }

        // 6. مركز المساعدة (Help Center)
        public IActionResult Help()
        {
            return View();
        }

        // 7. البحث الشامل الذكي
        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Dashboard");

            string q = query.Trim().ToLower();

            if (q.Contains("product") || q.Contains("منتج")) return RedirectToAction("Index", "Products");
            if (q.Contains("supplier") || q.Contains("مورد")) return RedirectToAction("Index", "Suppliers");
            if (q.Contains("contract") || q.Contains("عقد")) return RedirectToAction("Index", "SupplierProducts");
            if (q.Contains("categor") || q.Contains("تصنيف")) return RedirectToAction("Index", "Categories");
            if (q.Contains("purchase") || q.Contains("شراء")) return RedirectToAction("Index", "Purchases");
            if (q.Contains("sale") || q.Contains("بيع")) return RedirectToAction("Index", "Sales");
            if (q.Contains("analytic") || q.Contains("تحليل")) return RedirectToAction("Analytics");
            if (q.Contains("report") || q.Contains("تقرير")) return RedirectToAction("Reports");
            if (q.Contains("setting") || q.Contains("اعداد")) return RedirectToAction("Settings");
            if (q.Contains("help") || q.Contains("مساعد")) return RedirectToAction("Help");

            var productMatch = await _context.Products.FirstOrDefaultAsync(p => p.ProductName.ToLower().Contains(q));
            if (productMatch != null)
                return RedirectToAction("Index", "Products", new { searchString = query });

            TempData["SearchError"] = $"عذراً، لم يتم العثور على أي نتائج مطابقة لـ \"{query}\"!";
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/Home/Dashboard" : referer);
        }

        // 8. شات الذكاء الاصطناعي التفاعلي ثنائي اللغة
        [HttpPost]
        public async Task<IActionResult> AskAiAssistant([FromBody] string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return BadRequest("Message cannot be empty.");

            string aiReply = await CallLLMApiAsync(userMessage);
            return Json(new { reply = aiReply });
        }

        private async Task<string> CallLLMApiAsync(string userPrompt)
        {
            try
            {
                string? apiKey = _configuration["GroqApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey)) return GetFallbackReply(userPrompt);

                var products = await _context.Products
                    .Select(p => new { p.ProductName, p.StockQuantity, p.UnitPrice })
                    .ToListAsync();

                string inventoryData = products.Any()
                    ? JsonSerializer.Serialize(products)
                    : "No products currently registered in the database (0 products).";

                string url = "https://api.groq.com/openai/v1/chat/completions";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                string systemPrompt = $@"You are 'Stockora Copilot', an expert, polite, and professional AI assistant for the Stockora Inventory Management System.
- STRICT RULE: Always respond in the EXACT SAME LANGUAGE as the user's prompt (Arabic for Arabic prompts, English for English prompts).
- Only refer to real items found in the current inventory context: {inventoryData}. If the inventory is empty, clearly state that there are no products currently registered.
- When thanked ('شكرا' or 'thank you'), answer cordially.
- Guide users step-by-step through the application.";

                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    }
                };

                var response = await client.PostAsJsonAsync(url, requestBody);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                    return jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? GetFallbackReply(userPrompt);
                }
                return GetFallbackReply(userPrompt);
            }
            catch { return GetFallbackReply(userPrompt); }
        }

        private bool IsArabic(string text)
        {
            return Regex.IsMatch(text, @"\p{IsArabic}");
        }

        // محرك الردود الذكي (يعتمد على البيانات الفعلية فقط)
        private string GetFallbackReply(string prompt)
        {
            string q = prompt.ToLower().Trim();
            bool isAr = IsArabic(prompt);

            // 1. الشكر والامتنان
            if (q.Contains("شكر") || q.Contains("تسلم") || q.Contains("يعطيك العافية") || q.Contains("مشكور") ||
                q.Contains("thank") || q.Contains("thx") || q.Contains("appreciate"))
            {
                if (isAr)
                    return "على الرحب والسعة دائماً! 🌟 يسعدني ويشرفني تقديم المساعدة لك في أي وقت. إذا كانت لديك أي استفسارات أخرى، فأنا في خدمتك دائماً.";
                else
                    return "You are very welcome! 🌟 It is my pleasure to assist you. If you have any further questions or need help navigating Stockora, feel free to ask anytime!";
            }

            // 2. التحية والترحيب
            if (q.Contains("مرحبا") || q.Contains("اهلا") || q.Contains("السلام") || q.Contains("صباح") || q.Contains("مساء") ||
                q.Contains("hello") || q.Contains("hi") || q.Contains("hey") || q.Contains("good morning") || q.Contains("good evening"))
            {
                if (isAr)
                    return "أهلاً ومرحباً بك في Stockora! 👋\nأنا مساعدك الذكي لإدارة المخزون والعمليات. كيف يمكنني مساندتك اليوم في تيسير أعمالك؟";
                else
                    return "Hello and welcome to Stockora! 👋\nI am your intelligent inventory assistant. How may I assist you today with your inventory, suppliers, or operations?";
            }

            // 3. الاستفسار عن المنتجات المتوفرة في المخزن
            if (q.Contains("متوفر") || q.Contains("برودكت") || q.Contains("منتجات") || q.Contains("بضاعة") || q.Contains("اصناف") ||
                q.Contains("available") || q.Contains("items") || q.Contains("catalog") || q.Contains("stock list"))
            {
                var dbProducts = _context.Products.ToList();

                if (isAr)
                {
                    if (dbProducts.Any())
                    {
                        string rep = $"📦 المنتجات المسجلة حالياً في المستودع (العدد: {dbProducts.Count}):\n\n";
                        foreach (var item in dbProducts)
                        {
                            rep += $"• {item.ProductName} | الكمية المتاحة: {item.StockQuantity} | السعر: ${item.UnitPrice}\n";
                        }
                        rep += "\n💡 يمكنك مراجعة وتعديل بيانات المنتجات من قسم Products.";
                        return rep;
                    }
                    else
                    {
                        return "📦 لا توجد أي منتجات مسجلة في المستودع حالياً (المخزن فارغ).\n\n" +
                               "يمكنك البدء بإضافة منتجاتك بالدخول إلى قسم Products من القائمة الجانبية والضغط على Create New.";
                    }
                }
                else
                {
                    if (dbProducts.Any())
                    {
                        string rep = $"📦 Current products available in your inventory (Total: {dbProducts.Count}):\n\n";
                        foreach (var item in dbProducts)
                        {
                            rep += $"• {item.ProductName} | Stock: {item.StockQuantity} | Price: ${item.UnitPrice}\n";
                        }
                        rep += "\n💡 You can view and manage all inventory from the Products tab.";
                        return rep;
                    }
                    else
                    {
                        return "📦 There are no products registered in the database yet (Inventory is empty).\n\n" +
                               "You can add items by navigating to the Products section in the sidebar and clicking Create New.";
                    }
                }
            }

            // 4. استفسار عام عن إضافة أو تعديل المنتجات
            if (q.Contains("منتج") || q.Contains("اضيف") || q.Contains("اضافة") || q.Contains("product") || q.Contains("add item"))
            {
                int count = _context.Products.Count();
                if (isAr)
                {
                    return "لإدارة وإضافة المنتجات إلى المستودع:\n\n" +
                           "1️⃣ توجّه إلى نافذة Products عبر القائمة الجانبية.\n" +
                           "2️⃣ اضغط على خيار Create New لتسجيل صنف جديد مع تحديد الكمية، السعر، وحد الأمان للتنبيه.\n" +
                           $"📦 إجمالي المنتجات المسجلة حالياً: {count} صنف.";
                }
                else
                {
                    return "To add and manage products in the warehouse:\n\n" +
                           "1️⃣ Go to the Products module from the sidebar.\n" +
                           "2️⃣ Click 'Create New' to register a new product with stock counts, price, and low-stock alerts.\n" +
                           $"📦 Total items currently registered: {count}.";
                }
            }

            // 5. التسجيل وعمل حساب
            if (q.Contains("تسجيل") || q.Contains("حساب") || q.Contains("سجل") || q.Contains("اكاونت") ||
                q.Contains("register") || q.Contains("sign up") || q.Contains("create account") || q.Contains("join"))
            {
                if (isAr)
                {
                    return "يسعدنا انضمامك إلى Stockora! لإنشاء حساب جديد، يرجى اتباع الخطوات التالية:\n\n" +
                           "1️⃣ اضغط على زر Get Started في أعلى يمين الصفحة أو زر Create Account.\n" +
                           "2️⃣ أدخل بياناتك الكريمة (الاسم الكامل، البريد الإلكتروني، وكلمة المرور).\n" +
                           "3️⃣ اضغط Create Account، وسيصبح حسابك مفعّلاً فوراً لإدارة مخزونك بكل احترافية.";
                }
                else
                {
                    return "To create a new administrator account:\n\n" +
                           "1️⃣ Click 'Get Started' or 'Create Account' in the header.\n" +
                           "2️⃣ Enter your full name, email address, and password.\n" +
                           "3️⃣ Click Create Account, and you'll be redirected straight to your dashboard!";
                }
            }

            // 6. تسجيل الدخول
            if (q.Contains("دخول") || q.Contains("لوجن") || q.Contains("login") || q.Contains("sign in"))
            {
                if (isAr)
                {
                    return "للدخول إلى مساحة العمل الخاصة بك:\n\n" +
                           "1️⃣ انقر فوق زر Sign In المتواجد في الشريط العلوي.\n" +
                           "2️⃣ تفضل بإدخال بريدك الإلكتروني وكلمة المرور المسجلة.\n" +
                           "3️⃣ اضغط على Sign In للانتقال المباشر إلى لوحة التحكم الرئيسية (Dashboard).";
                }
                else
                {
                    return "To sign in:\n\n" +
                           "1️⃣ Click Sign In located at the top-right header.\n" +
                           "2️⃣ Enter your registered email address and password.\n" +
                           "3️⃣ Click Sign In to access the central dashboard.";
                }
            }

            // 7. النواقص وتنبيهات المخزون
            if (q.Contains("ناقص") || q.Contains("نواقص") || q.Contains("نقص") ||
                q.Contains("low stock") || q.Contains("alert") || q.Contains("threshold") || q.Contains("depleted"))
            {
                var lowProducts = _context.Products.Where(p => p.StockQuantity <= p.LowStockThreshold).ToList();
                if (isAr)
                {
                    if (lowProducts.Any())
                    {
                        string rep = $"⚠️ تقرير النواقص: هناك {lowProducts.Count} منتج(منتجات) تجاوزت حد الأمان وتتطلب إعادة التوريد:\n\n";
                        foreach (var lp in lowProducts.Take(5))
                        {
                            rep += $"• {lp.ProductName} (المتبقي: {lp.StockQuantity} | حد الأمان: {lp.LowStockThreshold})\n";
                        }
                        rep += "\n📄 يمكنك استخراج وطباعة التقرير الكامل من قسم Reports.";
                        return rep;
                    }
                    return "✨ حالة المخزون ممتازة! جميع الأصناف متوفرة بكميات كافية وتتجاوز حدود الأمان المحددة.";
                }
                else
                {
                    if (lowProducts.Any())
                    {
                        string rep = $"⚠️ Low-Stock Alert: {lowProducts.Count} product(s) are below safe thresholds:\n\n";
                        foreach (var lp in lowProducts.Take(5))
                        {
                            rep += $"• {lp.ProductName} (Current: {lp.StockQuantity} | Threshold: {lp.LowStockThreshold})\n";
                        }
                        rep += "\n📄 You can review the full summary in the Reports section.";
                        return rep;
                    }
                    return "✨ Inventory status is healthy! All items are adequately stocked.";
                }
            }

            // 8. الموردين
            if (q.Contains("مورد") || q.Contains("موردين") || q.Contains("supplier") || q.Contains("vendor"))
            {
                int supCount = _context.Suppliers.Count();
                if (isAr)
                {
                    return $"لإدارة الموردين وشبكة الإمداد:\n\n" +
                           "• من القائمة الجانبية، اختر Suppliers لإضافة وتعديل بيانات الموردين وجهات الاتصال.\n" +
                           "• تفضل بزيارة Supplier Contracts لربط الموردين بالمنتجات الخاصة بكل منهم.\n" +
                           $"🤝 عدد الموردين المعتمدين في النظام لديك: {supCount} مورد.";
                }
                else
                {
                    return $"To manage your vendors:\n\n" +
                           "• Open the Suppliers section to catalog vendor contacts.\n" +
                           "• Open Supplier Contracts to assign catalog products to suppliers.\n" +
                           $"🤝 Registered suppliers: {supCount}.";
                }
            }

            // 9. المبيعات
            if (q.Contains("بيع") || q.Contains("مبيعات") || q.Contains("ارباح") || q.Contains("sales") || q.Contains("revenue"))
            {
                decimal sales = _context.Sales.Any() ? _context.Sales.Sum(s => s.TotalAmount) : 0;
                if (isAr)
                {
                    return $"لمتابعة وتسجيل عمليات البيع:\n\n" +
                           "• انتقل إلى نافذة Sales لتسجيل المعاملات الجديدة، وسيتم خصم الكميات تلقائياً وبدقة من رصيد المخزن.\n" +
                           $"📈 إجمالي قيمة المبيعات المسجلة حتى الآن: ${sales:N0}.";
                }
                else
                {
                    return $"To manage sales:\n\n" +
                           "• Open the Sales module to issue invoices. Stock counts will be reduced automatically.\n" +
                           $"📈 Total recorded sales: ${sales:N0}.";
                }
            }

            // 10. المشتريات
            if (q.Contains("شراء") || q.Contains("مشتريات") || q.Contains("purchases") || q.Contains("order"))
            {
                decimal purchases = _context.Purchases.Any() ? _context.Purchases.Sum(p => p.TotalAmount) : 0;
                if (isAr)
                {
                    return $"لإدارة فواتير المشتريات والشحنات الواردة:\n\n" +
                           "• اختر Purchases من القائمة لتسجيل استلام البضائع وزيادة رصيد المستودع تلقائياً.\n" +
                           $"🛒 إجمالي تكاليف المشتريات المحسوبة: ${purchases:N0}.";
                }
                else
                {
                    return $"To track procurement:\n\n" +
                           "• Open Purchases to log inbound deliveries and increase inventory levels.\n" +
                           $"🛒 Total purchase costs: ${purchases:N0}.";
                }
            }

            // 11. شرح النظام
            if (q.Contains("اي هو") || q.Contains("ما هو") || q.Contains("شرح") || q.Contains("نظام") || q.Contains("stockora") ||
                q.Contains("what is") || q.Contains("about") || q.Contains("help") || q.Contains("features"))
            {
                if (isAr)
                {
                    return "منظومة Stockora هي الحل الذكي والمتكامل لإدارة المخازن والمستودعات بدقة وسلاسة، وتتيح لك:\n\n" +
                           "🔹 مراقبة وإدارة كميات المنتجات بشكل لحظي.\n" +
                           "🔹 إصدار تنبيهات آلية فورية عند انخفاض المخزون عن حد الأمان.\n" +
                           "🔹 تتبع كامل لدورة المبيعات وفواتير الشراء.\n" +
                           "🔹 استخراج وطباعة تقارير مالية ولوجستية تفصيلية.\n\n" +
                           "يسعدني توجيهك في أي قسم ترغب في استكشافه الآن.";
                }
                else
                {
                    return "Stockora is a next-generation intelligent inventory management platform built for modern operations:\n\n" +
                           "🔹 Real-time cataloging and tracking of items and quantities.\n" +
                           "🔹 Automatic low-stock warnings when inventory reaches safety thresholds.\n" +
                           "🔹 End-to-end sales and purchase transaction workflows.\n" +
                           "🔹 Live printable audit reports and performance analytics.\n\n" +
                           "Please let me know which area you would like to explore!";
                }
            }

            // 12. الرد الافتراضي
            if (isAr)
            {
                return "تحياتي لك! أنا في خدمتك دائماً لمساعدتك في كل ما يخص منصة Stockora.\n\n" +
                       "تفضل بإخباري بما ترغب في إنجازه، وسأقوم بشرح الخطوات وتوجيهك بكل سرور!";
            }
            else
            {
                return "Greetings! I am here to assist you with all aspects of the Stockora platform.\n\n" +
                       "Please let me know what task you'd like to accomplish, and I will gladly provide step-by-step guidance!";
            }
        }
    }
}