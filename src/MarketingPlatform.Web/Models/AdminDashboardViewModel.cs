namespace MarketingPlatform.Web.Models
{
    /// <summary>
    /// ViewModel for the Admin Dashboard
    /// </summary>
    public class AdminDashboardViewModel
    {
        public DashboardStats Stats { get; set; } = new DashboardStats();
        public List<RecentActivityItem> RecentActivities { get; set; } = new List<RecentActivityItem>();
        public List<CampaignSummary> RecentCampaigns { get; set; } = new List<CampaignSummary>();
        public List<UserSummary> RecentUsers { get; set; } = new List<UserSummary>();
    }

    /// <summary>
    /// Dashboard statistics
    /// </summary>
    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalCampaigns { get; set; }
        public int TotalContacts { get; set; }
        public decimal TotalRevenue { get; set; }
        public int MessagesSentToday { get; set; }
        public int MessagesSentThisMonth { get; set; }
        public double DeliveryRate { get; set; }
    }

    /// <summary>
    /// Recent activity item
    /// </summary>
    public class RecentActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Campaign summary for dashboard
    /// </summary>
    public class CampaignSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Recipients { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// User summary for dashboard
    /// </summary>
    public class UserSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
