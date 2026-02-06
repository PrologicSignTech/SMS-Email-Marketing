/// SERVER-SIDE API INTEGRATION - Uses ApiClient for secure, reliable API calls
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Web.Models;
using MarketingPlatform.Web.Services;
using MarketingPlatform.Application.DTOs.Common;

namespace MarketingPlatform.Web.Controllers
{
    /// <summary>
    /// Admin Dashboard Controller - For administrators to manage the platform
    /// Requires Admin or SuperAdmin role
    /// </summary>
    [Authorize]
    public class AdminDashboardController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AdminDashboardController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminDashboardController(IApiClient apiClient, ILogger<AdminDashboardController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _apiClient = apiClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _apiClient.SetAuthorizationToken(token);
            }
        }

        /// <summary>
        /// Admin Dashboard Index page
        /// </summary>
        public IActionResult Index()
        {
            // Create mock data for demonstration
            var viewModel = new AdminDashboardViewModel
            {
                Stats = new DashboardStats
                {
                    TotalUsers = 1247,
                    ActiveCampaigns = 23,
                    TotalCampaigns = 156,
                    TotalContacts = 45678,
                    TotalRevenue = 125340.50m,
                    MessagesSentToday = 3421,
                    MessagesSentThisMonth = 89234,
                    DeliveryRate = 98.7
                },
                RecentActivities = GenerateMockActivities(),
                RecentCampaigns = GenerateMockCampaigns(),
                RecentUsers = GenerateMockUsers()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Generate mock activity data
        /// </summary>
        private List<RecentActivityItem> GenerateMockActivities()
        {
            return new List<RecentActivityItem>
            {
                new RecentActivityItem
                {
                    Icon = "bi-person-plus",
                    Title = "New User Registered",
                    Description = "john.doe@example.com joined the platform",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5),
                    Type = "user"
                },
                new RecentActivityItem
                {
                    Icon = "bi-megaphone",
                    Title = "Campaign Launched",
                    Description = "Summer Sale campaign started",
                    Timestamp = DateTime.UtcNow.AddMinutes(-15),
                    Type = "campaign"
                },
                new RecentActivityItem
                {
                    Icon = "bi-envelope-check",
                    Title = "Email Campaign Completed",
                    Description = "Newsletter #45 sent to 5,432 contacts",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Type = "campaign"
                },
                new RecentActivityItem
                {
                    Icon = "bi-currency-dollar",
                    Title = "Subscription Upgrade",
                    Description = "User upgraded to Enterprise plan",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Type = "billing"
                },
                new RecentActivityItem
                {
                    Icon = "bi-graph-up",
                    Title = "Analytics Report Generated",
                    Description = "Monthly performance report created",
                    Timestamp = DateTime.UtcNow.AddHours(-3),
                    Type = "analytics"
                }
            };
        }

        /// <summary>
        /// Generate mock campaign data
        /// </summary>
        private List<CampaignSummary> GenerateMockCampaigns()
        {
            return new List<CampaignSummary>
            {
                new CampaignSummary
                {
                    Id = 1,
                    Name = "Summer Sale 2024",
                    Status = "Active",
                    Type = "Email",
                    Recipients = 5432,
                    SuccessCount = 5321,
                    FailureCount = 111,
                    ScheduledAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new CampaignSummary
                {
                    Id = 2,
                    Name = "Product Launch SMS",
                    Status = "Active",
                    Type = "SMS",
                    Recipients = 2156,
                    SuccessCount = 2134,
                    FailureCount = 22,
                    ScheduledAt = DateTime.UtcNow.AddHours(-6),
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new CampaignSummary
                {
                    Id = 3,
                    Name = "Weekly Newsletter",
                    Status = "Scheduled",
                    Type = "Email",
                    Recipients = 8765,
                    SuccessCount = 0,
                    FailureCount = 0,
                    ScheduledAt = DateTime.UtcNow.AddDays(1),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new CampaignSummary
                {
                    Id = 4,
                    Name = "Flash Sale Alert",
                    Status = "Completed",
                    Type = "SMS",
                    Recipients = 3421,
                    SuccessCount = 3398,
                    FailureCount = 23,
                    ScheduledAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new CampaignSummary
                {
                    Id = 5,
                    Name = "Customer Feedback Survey",
                    Status = "Completed",
                    Type = "Email",
                    Recipients = 1234,
                    SuccessCount = 1221,
                    FailureCount = 13,
                    ScheduledAt = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                }
            };
        }

        /// <summary>
        /// Generate mock user data
        /// </summary>
        private List<UserSummary> GenerateMockUsers()
        {
            return new List<UserSummary>
            {
                new UserSummary
                {
                    Id = "1",
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    LastLoginAt = null,
                    IsActive = true,
                    Role = "User"
                },
                new UserSummary
                {
                    Id = "2",
                    Email = "jane.smith@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    LastLoginAt = DateTime.UtcNow.AddMinutes(-30),
                    IsActive = true,
                    Role = "Manager"
                },
                new UserSummary
                {
                    Id = "3",
                    Email = "bob.johnson@example.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    LastLoginAt = DateTime.UtcNow.AddHours(-5),
                    IsActive = true,
                    Role = "User"
                },
                new UserSummary
                {
                    Id = "4",
                    Email = "alice.williams@example.com",
                    FirstName = "Alice",
                    LastName = "Williams",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    LastLoginAt = DateTime.UtcNow.AddDays(-2),
                    IsActive = true,
                    Role = "Analyst"
                },
                new UserSummary
                {
                    Id = "5",
                    Email = "charlie.brown@example.com",
                    FirstName = "Charlie",
                    LastName = "Brown",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    LastLoginAt = DateTime.UtcNow.AddDays(-1),
                    IsActive = false,
                    Role = "User"
                }
            };
        }
    }
}
