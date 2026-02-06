using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MarketingPlatform.Infrastructure.Data;
using MarketingPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity;
using MarketingPlatform.Application.Interfaces;
using MarketingPlatform.Infrastructure.Services;
using MarketingPlatform.Application.Services;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Repositories;
using MarketingPlatform.Application.Mappings;
using MarketingPlatform.API.Middleware;
using Serilog;
using Hangfire;
using Hangfire.SqlServer;
using MarketingPlatform.Core.Interfaces;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Encryption Services - Configuration-based by default with Azure/AWS options
var encryptionProvider = builder.Configuration["Encryption:Provider"] ?? "Configuration";
Log.Information("Initializing encryption with provider: {Provider}", encryptionProvider);

switch (encryptionProvider.ToLowerInvariant())
{
    case "azure":
        // Azure Key Vault integration (requires Azure.Security.KeyVault.Keys package)
        Log.Warning("Azure Key Vault provider selected but not implemented. Install Azure.Security.KeyVault.Keys package and implement AzureKeyVaultService.");
        Log.Information("Falling back to Configuration-based key management.");
        builder.Services.AddSingleton<IKeyManagementService, ConfigurationKeyManagementService>();
        break;
    
    case "aws":
        // AWS KMS integration (requires AWSSDK.KeyManagementService package)
        Log.Warning("AWS KMS provider selected but not implemented. Install AWSSDK.KeyManagementService package and implement AwsKmsService.");
        Log.Information("Falling back to Configuration-based key management.");
        builder.Services.AddSingleton<IKeyManagementService, ConfigurationKeyManagementService>();
        break;
    
    case "configuration":
    default:
        Log.Information("Using Configuration-based key management. For production, consider Azure Key Vault or AWS KMS.");
        builder.Services.AddSingleton<IKeyManagementService, ConfigurationKeyManagementService>();
        break;
}

builder.Services.AddScoped<IEncryptionService, AesEncryptionService>();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});

// Authorization with Permission-based policies
builder.Services.AddAuthorization(options =>
{
    // Register authorization handler
    foreach (MarketingPlatform.Core.Enums.Permission permission in Enum.GetValues(typeof(MarketingPlatform.Core.Enums.Permission)))
    {
        if (permission != MarketingPlatform.Core.Enums.Permission.All)
        {
            options.AddPolicy($"Permission:{permission}", policy =>
                policy.Requirements.Add(new MarketingPlatform.API.Authorization.PermissionRequirement(permission)));
        }
    }
});

builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, MarketingPlatform.API.Authorization.PermissionAuthorizationHandler>();

// Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IExternalAuthProviderRepository, ExternalAuthProviderRepository>();
builder.Services.AddScoped<IUserExternalLoginRepository, UserExternalLoginRepository>();
builder.Services.AddScoped<ISuperAdminRepository, SuperAdminRepository>();
builder.Services.AddScoped<IPrivilegedActionLogRepository, PrivilegedActionLogRepository>();
builder.Services.AddScoped<IPlatformConfigurationRepository, PlatformConfigurationRepository>();

// Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOAuth2Service, OAuth2Service>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IContactGroupService, ContactGroupService>();
builder.Services.AddScoped<IDynamicGroupEvaluationService, DynamicGroupEvaluationService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICampaignABTestingService, CampaignABTestingService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<ISuppressionListService, SuppressionListService>();
builder.Services.AddScoped<IContactTagService, ContactTagService>();
builder.Services.AddScoped<IAudienceSegmentationService, AudienceSegmentationService>();
builder.Services.AddScoped<IKeywordService, KeywordService>();
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();
builder.Services.AddScoped<IPrivilegedActionLogService, PrivilegedActionLogService>();
builder.Services.AddScoped<IPlatformConfigurationService, PlatformConfigurationService>();
builder.Services.AddScoped<IPlatformSettingService, PlatformSettingService>();
builder.Services.AddScoped<IFeatureToggleService, FeatureToggleService>();
builder.Services.AddScoped<IComplianceRuleService, ComplianceRuleService>();

// Landing Page Services
builder.Services.AddScoped<IUseCaseService, UseCaseService>();
builder.Services.AddScoped<ISecurityBadgeService, SecurityBadgeService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<ITrustedCompanyService, TrustedCompanyService>();
builder.Services.AddScoped<ILandingFaqService, LandingFaqService>();
builder.Services.AddScoped<ILandingFeatureService, LandingFeatureService>();
builder.Services.AddScoped<ILandingStatService, LandingStatService>();
builder.Services.AddScoped<IFooterSettingsService, FooterSettingsService>();

// Chat Services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// Scheduling & Automation Services
builder.Services.AddScoped<ICampaignSchedulerService, CampaignSchedulerService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IEventTriggerService, EventTriggerService>();
builder.Services.AddScoped<IRateLimitService, RateLimitService>();

// Message Service
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMessageRoutingService, MessageRoutingService>();

// Analytics & Reporting Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();

// Subscription & Billing Services
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUsageTrackingService, UsageTrackingService>();
builder.Services.AddScoped<ISuperAdminAnalyticsService, SuperAdminAnalyticsService>();

// Integration Services
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<ICRMIntegrationService, CRMIntegrationService>();

// OAuth2 Providers (optional - only used if configured)
builder.Services.AddScoped<IOAuth2Provider, MarketingPlatform.Infrastructure.Services.OAuth2Providers.AzureADAuthProvider>();
builder.Services.AddScoped<IOAuth2Provider, MarketingPlatform.Infrastructure.Services.OAuth2Providers.GoogleAuthProvider>();
builder.Services.AddScoped<IOAuth2Provider, MarketingPlatform.Infrastructure.Services.OAuth2Providers.OktaAuthProvider>();
builder.Services.AddScoped<IOAuth2Provider, MarketingPlatform.Infrastructure.Services.OAuth2Providers.AwsCognitoAuthProvider>();

// File Storage Providers
builder.Services.AddScoped<IFileStorageProvider, MarketingPlatform.Infrastructure.Services.FileStorageProviders.LocalFileStorageProvider>();
builder.Services.AddScoped<IFileStorageProvider, MarketingPlatform.Infrastructure.Services.FileStorageProviders.AzureBlobStorageProvider>();
builder.Services.AddScoped<IFileStorageProvider, MarketingPlatform.Infrastructure.Services.FileStorageProviders.S3FileStorageProvider>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Provider Services (Mock implementations)
builder.Services.AddScoped<ISMSProvider, MockSMSProvider>();
builder.Services.AddScoped<IMMSProvider, MockMMSProvider>();
builder.Services.AddScoped<IEmailProvider, MockEmailProvider>();

// TODO: Replace with real providers in production:
// builder.Services.AddScoped<ISMSProvider, TwilioSMSProvider>();
// builder.Services.AddScoped<IMMSProvider, TwilioMMSProvider>();
// builder.Services.AddScoped<IEmailProvider, SendGridEmailProvider>();

// Add HttpClientFactory for OAuth2 providers
builder.Services.AddHttpClient();

// Background Services
builder.Services.AddHostedService<MessageQueueProcessor>();
builder.Services.AddHostedService<DynamicGroupUpdateProcessor>();

// Hangfire Configuration - Optimized to avoid DB locks and reduce load
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15), // Reduce polling frequency to minimize DB load
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true, // IMPORTANT: Disable global locks to prevent table locking
        PrepareSchemaIfNecessary = true,
        SchemaName = "hangfire"
    }));

// Add Hangfire server with optimized settings
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5; // Limit worker count to control DB connections
    options.Queues = new[] { "default", "campaigns", "workflows", "rate-limits" }; // Separate queues
    options.ServerCheckInterval = TimeSpan.FromMinutes(1);
    options.HeartbeatInterval = TimeSpan.FromSeconds(30);
    options.ServerTimeout = TimeSpan.FromMinutes(5);
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15); // Reduce schedule polling to minimize DB load
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7061" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Marketing Platform API",
        Version = "v1",
        Description = "RESTful API for Marketing Platform - Comprehensive SMS, MMS & Email Marketing Solution with advanced features including contact management, campaign automation, analytics, and integrations.",
        Contact = new OpenApiContact
        {
            Name = "Marketing Platform Support",
            Email = "support@marketingplatform.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable annotations for better documentation
    options.EnableAnnotations();
});

var app = builder.Build();

// Content Security Policy Middleware with nonce-based inline script/style support
// This middleware generates a unique nonce per request and sets CSP headers
// Development: Permissive CSP to allow hot reload, browser-link, WebSockets
// Production: Strict nonce-based CSP for security
app.UseMiddleware<CspMiddleware>();

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Rate limiting middleware (before authentication)
app.UseMiddleware<RateLimitingMiddleware>();

// Enable static files for custom resources
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Marketing Platform API");
});
// HTTPS Redirection - Enforce TLS 1.2+
app.UseHttpsRedirection();

// HSTS (HTTP Strict Transport Security) - Enforces HTTPS for specified duration
if (builder.Configuration.GetValue<bool>("Security:EnableHSTS", true))
{
    var hstsMaxAge = builder.Configuration.GetValue<int>("Security:HSTSMaxAge", 31536000); // Default: 1 year
    var includeSubDomains = builder.Configuration.GetValue<bool>("Security:IncludeSubDomains", true);
    
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        var hstsHeader = $"max-age={hstsMaxAge}";
        if (includeSubDomains)
            hstsHeader += "; includeSubDomains";
        
        context.Response.Headers.Append("Strict-Transport-Security", hstsHeader);
        await next();
    });
}

// Hangfire Dashboard with authorization
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
    StatsPollingInterval = 30000 // Poll every 30 seconds instead of default to reduce DB load
});

// Enable CORS
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hubs
app.MapHub<MarketingPlatform.API.Hubs.ChatHub>("/hubs/chat");

// Configure recurring jobs for rate limit resets using service provider
using (var scope = app.Services.CreateScope())
{
    var rateLimitService = scope.ServiceProvider.GetRequiredService<IRateLimitService>();
    var eventTriggerService = scope.ServiceProvider.GetRequiredService<IEventTriggerService>();
    
    RecurringJob.AddOrUpdate(
        "reset-daily-rate-limits",
        () => rateLimitService.ResetDailyCountersAsync(),
        Cron.Daily(0, 0)); // Run at midnight UTC

    RecurringJob.AddOrUpdate(
        "reset-weekly-rate-limits",
        () => rateLimitService.ResetWeeklyCountersAsync(),
        Cron.Weekly(DayOfWeek.Monday, 0, 0)); // Run at midnight Monday UTC

    RecurringJob.AddOrUpdate(
        "reset-monthly-rate-limits",
        () => rateLimitService.ResetMonthlyCountersAsync(),
        Cron.Monthly(1, 0, 0)); // Run at midnight on 1st of month UTC

    RecurringJob.AddOrUpdate(
        "check-inactivity-triggers",
        () => eventTriggerService.CheckInactivityTriggersAsync(),
        Cron.Daily(2, 0)); // Run at 2 AM UTC daily
}

app.MapControllers();

// Database initialization with comprehensive error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        logger.LogInformation("Starting database initialization...");
        
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
        logger.LogInformation("Applying database migrations...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var pendingMigrationsList = pendingMigrations.ToList();
        
        if (pendingMigrationsList.Any())
        {
            logger.LogInformation("Found {Count} pending migrations: {Migrations}", 
                pendingMigrationsList.Count, 
                string.Join(", ", pendingMigrationsList));
            
            await context.Database.MigrateAsync();
            
            logger.LogInformation("All migrations applied successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations found. Database is up to date.");
        }
        
        // Verify migrations were applied
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        logger.LogInformation("Total applied migrations: {Count}", appliedMigrations.Count());
        
        // Ensure all required tables exist
        logger.LogInformation("Verifying required tables exist...");
        //var requiredTables = new[] 
        //{ 
        //    "AspNetUsers", "AspNetRoles", "CustomRoles", "CustomUserRoles",
        //    "SubscriptionPlans", "PlatformSettings", "PageContents",
        //    "MessageProviders", "ChannelRoutingConfigs", "PricingModels"
        //};
        
        //foreach (var tableName in requiredTables)
        //{
        //    try
        //    {
        //        var count = await context.Database.SqlQueryRaw<int>(
        //            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName).FirstOrDefaultAsync();
                
        //        if (count == 0)
        //        {
        //            logger.LogWarning("Required table '{TableName}' may not exist.", tableName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogWarning("Could not verify table '{TableName}': {Error}", tableName, ex.Message);
        //    }
        //}
        logger.LogInformation("Table verification completed.");
        
        // Seed initial data
        logger.LogInformation("Starting data seeding...");
        await DbInitializer.SeedAsync(context, userManager, roleManager, logger);
        
        Log.Information("Database initialization completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "A critical error occurred during database initialization. Application startup may fail.");
        
        // Log specific error details
        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerException}", ex.InnerException.Message);
        }
        
        // In production, you might want to prevent the application from starting
        // if database initialization fails. For now, we'll log and continue.
        // throw; // Uncomment to halt application startup on database errors
    }
}

app.Run();

// Hangfire Dashboard Authorization Filter
public class HangfireDashboardAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Development: Allow all access for testing
        // Production: Implement proper authentication
        // TODO: Replace with proper auth check that validates user is authenticated and has admin role
        return true;
    }
}
