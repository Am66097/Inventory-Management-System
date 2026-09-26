using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //  تسجيل قاعدة البيانات 
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            // Seed Initial Data (Data Seeding)
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<WebApplication1.Data.ApplicationDbContext>();

                // التأكد من وجود قسم ومنتج
                if (!context.Categories.Any())
                {
                    var category = new WebApplication1.Models.Category
                    {
                        CategoryName = "Electronics",
                        Description = "Laptops and gadgets"
                    };
                    context.Categories.Add(category);
                    context.SaveChanges();

                    if (!context.Products.Any())
                    {
                        context.Products.Add(new WebApplication1.Models.Product
                        {
                            SKU = "DELL-XPS-001",
                            ProductName = "Laptop Dell XPS 15",
                            CategoryID = category.CategoryId,
                            UnitPrice = 1500.00m,
                            StockQuantity = 10,
                            LowStockThreshold = 2
                        });
                        context.SaveChanges();
                    }
                }
            }

            app.Run();
        }
    }
}