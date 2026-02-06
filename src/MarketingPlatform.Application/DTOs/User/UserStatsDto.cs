namespace MarketingPlatform.Application.DTOs.User
{
    public class UserStatsDto
    {
        public int TotalCampaigns { get; set; }
        public int TotalContacts { get; set; }
        public int TotalMessagesSent { get; set; }
        public int ActiveCampaigns { get; set; }
        public decimal TotalSpent { get; set; }
        public string SubscriptionPlan { get; set; } = "Free";
    }
}
