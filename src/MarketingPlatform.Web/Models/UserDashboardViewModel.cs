namespace MarketingPlatform.Web.Models
{
    /// <summary>
    /// ViewModel for the User Dashboard
    /// </summary>
    public class UserDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public UserStats Stats { get; set; } = new UserStats();
        public List<CampaignSummary> MyCampaigns { get; set; } = new List<CampaignSummary>();
        public List<RecentActivityItem> RecentActivities { get; set; } = new List<RecentActivityItem>();
        public UserAnalytics Analytics { get; set; } = new UserAnalytics();
    }

    /// <summary>
    /// User-specific statistics
    /// </summary>
    public class UserStats
    {
        public int TotalCampaigns { get; set; }
        public int ActiveCampaigns { get; set; }
        public int TotalContacts { get; set; }
        public int MessagesSent { get; set; }
        public double EngagementRate { get; set; }
    }

    /// <summary>
    /// User analytics data
    /// </summary>
    public class UserAnalytics
    {
        public List<MonthlyData> MonthlyMessages { get; set; } = new List<MonthlyData>();
        public List<CampaignPerformance> TopCampaigns { get; set; } = new List<CampaignPerformance>();
    }

    /// <summary>
    /// Monthly data for charts
    /// </summary>
    public class MonthlyData
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// Campaign performance data
    /// </summary>
    public class CampaignPerformance
    {
        public string CampaignName { get; set; } = string.Empty;
        public double SuccessRate { get; set; }
        public int TotalSent { get; set; }
    }
}
