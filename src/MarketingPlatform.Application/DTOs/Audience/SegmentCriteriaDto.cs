namespace MarketingPlatform.Application.DTOs.Audience
{
    public class SegmentCriteriaDto
    {
        public List<SegmentRuleDto> Rules { get; set; } = new List<SegmentRuleDto>();
        public string LogicalOperator { get; set; } = "AND"; // AND or OR
    }

    public class SegmentRuleDto
    {
        public string Field { get; set; } = string.Empty; // e.g., "Country", "City", "Tag", "CustomAttribute.Key"
        public string Operator { get; set; } = string.Empty; // e.g., "Equals", "Contains", "GreaterThan", "In"
        public string Value { get; set; } = string.Empty;
    }
}
