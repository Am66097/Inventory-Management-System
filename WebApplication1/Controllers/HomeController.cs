using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text.Json;
using System.Net.Http.Json;

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

        public async Task<IActionResult> Index()
        {
            var dashboardData = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalSalesRevenue = await _context.Sales.AnyAsync() ? await _context.Sales.SumAsync(s => s.TotalAmount) : 0,
                TotalPurchasesCost = await _context.Purchases.AnyAsync() ? await _context.Purchases.SumAsync(p => p.TotalAmount) : 0,
                LowStockProducts = await _context.Products.Where(p => p.StockQuantity <= p.LowStockThreshold).ToListAsync()
            };

            return View(dashboardData);
        }

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
                // 1. قراءة مفتاح Groq
                string? apiKey = _configuration["GroqApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return "عذراً، لم يتم العثور على مفتاح Groq في الإعدادات.";
                }

                // 2. جلب بيانات المخزن
                var products = await _context.Products
                    .Select(p => new { p.ProductName, p.StockQuantity, p.UnitPrice })
                    .ToListAsync();

                string inventoryData = JsonSerializer.Serialize(products);

                // 3. تجهيز الطلب لمنصة Groq
                string url = "https://api.groq.com/openai/v1/chat/completions";
                using var client = new HttpClient();

                // إضافة المفتاح في الـ Header (طريقة Groq و OpenAI)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "openai/gpt-oss-20b",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful inventory assistant. Answer strictly based on this JSON data: " + inventoryData + ". Respond concisely and in the same language as the user." },
                        new { role = "user", content = userPrompt }
                    }
                };

                // 4. إرسال الطلب
                var response = await client.PostAsJsonAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

                    // استخراج النص من رد Groq
                    string aiText = jsonResponse
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content").GetString() ?? "عذراً، لم أتمكن من صياغة إجابة.";

                    return aiText;
                }
                else
                {
                    // لو حصل أي خطأ هيطبعلك السبب الفعلي
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    return $"سبب الرفض ({response.StatusCode}): {errorDetails}";
                }
            }
            catch (Exception ex)
            {
                return $"حدث خطأ في النظام: {ex.Message}";
            }
        }
    }
}