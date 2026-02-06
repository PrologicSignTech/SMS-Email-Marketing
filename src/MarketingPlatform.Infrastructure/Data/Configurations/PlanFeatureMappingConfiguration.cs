using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PlanFeatureMappingConfiguration : IEntityTypeConfiguration<PlanFeatureMapping>
    {
        public void Configure(EntityTypeBuilder<PlanFeatureMapping> builder)
        {
            builder.HasKey(pfm => pfm.Id);

            builder.Property(pfm => pfm.FeatureValue)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(pfm => pfm.SubscriptionPlan)
                .WithMany(sp => sp.PlanFeatureMappings)
                .HasForeignKey(pfm => pfm.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pfm => pfm.Feature)
                .WithMany(f => f.PlanFeatureMappings)
                .HasForeignKey(pfm => pfm.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pfm => pfm.SubscriptionPlanId);
            builder.HasIndex(pfm => pfm.FeatureId);
            builder.HasIndex(pfm => new { pfm.SubscriptionPlanId, pfm.FeatureId }).IsUnique();
            builder.HasIndex(pfm => pfm.DisplayOrder);
        }
    }
}
