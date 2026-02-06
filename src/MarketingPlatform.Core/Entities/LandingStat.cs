namespace MarketingPlatform.Core.Entities
{
    /// <summary>
    /// Represents a statistic displayed on the landing page
    /// </summary>
    public class LandingStat : BaseEntity
    {
        /// <summary>
        /// The display value (e.g., "10M+", "98%", "24/7")
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The label below the value (e.g., "Messages Delivered", "Success Rate")
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Optional description or subtitle
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Bootstrap icon class (e.g., "bi-envelope-paper", "bi-check-circle")
        /// </summary>
        public string IconClass { get; set; } = "bi-graph-up";

        /// <summary>
        /// Color class for styling (e.g., "primary", "success", "info")
        /// </summary>
        public string ColorClass { get; set; } = "primary";

        /// <summary>
        /// Numeric target for counter animation (optional)
        /// If set, the frontend can animate counting up to this number
        /// </summary>
        public long? CounterTarget { get; set; }

        /// <summary>
        /// Suffix to append after counter (e.g., "+", "%", "K")
        /// </summary>
        public string? CounterSuffix { get; set; }

        /// <summary>
        /// Prefix to prepend before counter (e.g., "$", ">")
        /// </summary>
        public string? CounterPrefix { get; set; }

        /// <summary>
        /// Display order on the landing page
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Whether this stat is active and should be displayed
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Whether to show on the landing page
        /// </summary>
        public bool ShowOnLanding { get; set; } = true;
    }
}
