namespace MarketingPlatform.Core.Entities
{
    public class Contact : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? CustomAttributes { get; set; } // JSON
        public bool IsActive { get; set; } = true;

        // Channel-specific consent flags
        public bool SmsOptIn { get; set; } = false;
        public bool MmsOptIn { get; set; } = false;
        public bool EmailOptIn { get; set; } = false;
        public DateTime? SmsOptInDate { get; set; }
        public DateTime? MmsOptInDate { get; set; }
        public DateTime? EmailOptInDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<ContactGroupMember> GroupMembers { get; set; } = new List<ContactGroupMember>();
        public virtual ICollection<ContactTagAssignment> TagAssignments { get; set; } = new List<ContactTagAssignment>();
        public virtual ICollection<ConsentHistory> ConsentHistories { get; set; } = new List<ConsentHistory>();
        public virtual ICollection<ContactConsent> ContactConsents { get; set; } = new List<ContactConsent>();
        public virtual ICollection<CampaignMessage> CampaignMessages { get; set; } = new List<CampaignMessage>();
        public virtual ContactEngagement? Engagement { get; set; }
    }
}
