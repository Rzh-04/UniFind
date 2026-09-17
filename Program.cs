using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UniversityLostAndFound.Data;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database Provider (Defaults to SQLite for instant local execution without requiring LocalDB installation)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var sqliteConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=UniversityLostAndFound.db";
var uploadsPath = builder.Configuration["UploadsPath"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=UniversityLostAndFoundDB;Trusted_Connection=True;MultipleActiveResultSets=true";
        options.UseSqlServer(sqlServerConnection);
    }
    else
    {
        options.UseSqlite(sqliteConnection);
    }
});

// 2. Configure Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings for easy beginner testing
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Login Cookie Paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// 3. Register Application Services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// 4. Add MVC Controllers & Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. Automatic Database Schema Creation & Data Seeding on Application Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
        throw;
    }
}

// 6. Configure HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

if (!string.IsNullOrWhiteSpace(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
