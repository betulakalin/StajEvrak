using Microsoft.EntityFrameworkCore;
using StajEvrak.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using StajEvrak.Core.Entities;
using StajEvrak.Core.Interfaces;
using StajEvrak.Infrastructure.Security;
using StajEvrak.Infrastructure.Services;
using StajEvrak.Infrastructure.Dataset;
using StajEvrak.Infrastructure.Files;
using StajEvrak.Infrastructure.Readers;
namespace StajEvrak.web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
            builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<IDatasetReader, ExcelDatasetReader>();
            builder.Services.AddScoped<IDatasetReader, CsvDatasetReader>();
            builder.Services.AddScoped<DatasetService>();
            builder.Services.AddScoped<IFileValidator, DocumentFileValidator>();
            builder.Services.AddScoped(sp => new DocumentService(
                sp.GetRequiredService<AppDbContext>(),
                sp.GetRequiredService<IFileValidator>(),
                Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads")));
            builder.Services.AddScoped<IEk16Reader, DocxEk16Reader>();
            builder.Services.AddScoped<IEk16Reader, PdfEk16Reader>();
            builder.Services.AddScoped<Ek16Service>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = "/Account/Login";
                    o.AccessDeniedPath = "/Account/AccessDenied";
                    o.LogoutPath = "/Account/Logout";
                    o.ExpireTimeSpan = TimeSpan.FromHours(2);
                });
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                if (!db.Admins.Any())
                {
                    db.Admins.Add(new Admin("admin", "admin@stajevrak.com", hasher.Hash("Admin123!")));
                    db.SaveChanges();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
