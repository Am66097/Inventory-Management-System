using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class AiAssistantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AiAssistantController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // عرض واجهة الشات
        public IActionResult Index()
        {
            return View();
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
                string? apiKey = _configuration["GroqApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return "عذراً، لم يتم العثور على مفتاح Groq في الإعدادات.";
                }

                var products = await _context.Products
                    .Select(p => new { p.ProductName, p.StockQuantity, p.UnitPrice })
                    .ToListAsync();

                string inventoryData = JsonSerializer.Serialize(products);
                string url = "https://api.groq.com/openai/v1/chat/completions";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "openai/gpt-oss-20b", // تأكد من اسم الموديل الصحيح في Groq
                    messages = new[]
                    {
                        new { role = "system", content = "You are a helpful inventory assistant. Answer strictly based on this JSON data: " + inventoryData + ". Respond concisely and in the same language as the user." },
                        new { role = "user", content = userPrompt }
                    }
                };

                var response = await client.PostAsJsonAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                    string aiText = jsonResponse
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content").GetString() ?? "عذراً، لم أتمكن من صياغة إجابة.";

                    return aiText;
                }
                else
                {
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