using Microsoft.EntityFrameworkCore;
using MarketingPlatform.Infrastructure.Data;
using MarketingPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Infrastructure.Services;
using MarketingPlatform.Application.Services;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Repositories;
using MarketingPlatform.Web;
using MarketingPlatform.Web.Middleware;
using MarketingPlatform.Web.Extensions;
using MarketingPlatform.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Cookie Authentication (for web application sessions)
// Note: We're NOT using Identity here - authentication is handled by the API
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "MarketingPlatform.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow both HTTP and HTTPS for development
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Increased to 8 hours
        options.SlidingExpiration = true;
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";
        options.ReturnUrlParameter = "returnUrl";
        options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                // Keep the session alive as long as the user is active
                var currentUtc = DateTimeOffset.UtcNow;
                var issuedUtc = context.Properties.IssuedUtc;

                if (issuedUtc.HasValue)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    var timeRemaining = context.Properties.ExpiresUtc.Value.Subtract(currentUtc);

                    // Refresh the cookie if more than half the time has elapsed
                    if (timeElapsed > timeRemaining)
                    {
                        context.ShouldRenew = true;
                    }
                }
                await Task.CompletedTask;
            }
        };
    });

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPageContentRepository, PageContentRepository>();

// Application Services
builder.Services.AddScoped<IPageContentService, PageContentService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// File Storage Providers
builder.Services.AddScoped<IFileStorageProvider, MarketingPlatform.Infrastructure.Services.FileStorageProviders.LocalFileStorageProvider>();

// HTTP Client Services (Server-side API communication)
builder.Services.AddHttpClient<IApiClient, ApiClient>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICampaignApiService, CampaignApiService>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Apply kebab-case transformation to route parameters
    options.Conventions.Add(new Microsoft.AspNetCore.Mvc.ApplicationModels.RouteTokenTransformerConvention(
        new SlugifyParameterTransformer()));
});
builder.Services.AddHttpContextAccessor();

// Configure routing to use lowercase URLs with kebab-case
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

var app = builder.Build();

// Content Security Policy Middleware with nonce-based inline script/style support
// This middleware generates a unique nonce per request and sets CSP headers
// Development: Permissive CSP to allow hot reload, browser-link, WebSockets
// Production: Strict nonce-based CSP for security
//app.UseMiddleware<CspMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("Starting database initialization for Web application...");
        
        // Test database connection
        logger.LogInformation("Testing database connection...");
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to the database. Please check your connection string and ensure SQL Server is running.");
            throw new InvalidOperationException("Database connection failed");
        }
        logger.LogInformation("Database connection successful.");
        
        // Apply pending migrations
        logger.LogInformation("Checking for pending migrations...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingMigrationsList = pendingMigrations.ToList();
        
        if (pendingMigrationsList.Any())
        {
            logger.LogInformation("Found {Count} pending migrations. Applying...", pendingMigrationsList.Count);
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations. Database is up to date.");
        }
        
        // Seed page content (Privacy Policy and Terms of Service)
        logger.LogInformation("Seeding page content...");
        await DatabaseSeeder.SeedPageContentAsync(scope.ServiceProvider);
        logger.LogInformation("Page content seeding completed.");
        
        logger.LogInformation("Database initialization for Web application completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
        
        // Log specific error details
        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
        }
        
        // Continue running even if seeding fails (tables should exist from API)
        // throw; // Uncomment to halt application on database errors
    }
}

app.Run();
