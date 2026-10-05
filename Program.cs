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

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebApplication1.Data.ApplicationDbContext>();
                //db.Database.EnsureDeleted(); // يحذف الداتابيز الفارغة القديمة لفك أي قفل
                db.Database.EnsureCreated(); // يعيد إنشاءها فوراً بكل الجداول: Products, Suppliers, etc.
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<WebApplication1.Data.ApplicationDbContext>();
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database connection issue: {ex.Message}");
                }
            }

            app.Run();
        }
    }
}