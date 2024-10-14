using Microsoft.EntityFrameworkCore;
using Bible.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Bible;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpClient();
        builder.Services.AddDbContext<ApplicationDbContext>(options => 
            options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 3, 0))));
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/SignIn";
                options.LogoutPath = "/SignIn";
                // options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // Use secure cookies based on the request
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;  // Allow non-HTTPS cookies in development
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
        builder.Services.AddAuthorization();

        Env.Load(); // !IMPORTANT LOAD VARIABLES FROM ENVIRONMENT
        var app = builder.Build();
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Introduction}/{action=Index}/{id?}");

        app.Run();
    }
}