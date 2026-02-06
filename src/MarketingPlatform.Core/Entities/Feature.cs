namespace MarketingPlatform.Core.Entities
{
    public class Feature : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<PlanFeatureMapping> PlanFeatureMappings { get; set; } = new List<PlanFeatureMapping>();
    }
}
