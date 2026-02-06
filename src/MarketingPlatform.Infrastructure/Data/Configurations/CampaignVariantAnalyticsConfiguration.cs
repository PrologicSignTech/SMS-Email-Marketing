using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignVariantAnalyticsConfiguration : IEntityTypeConfiguration<CampaignVariantAnalytics>
    {
        public void Configure(EntityTypeBuilder<CampaignVariantAnalytics> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.DeliveryRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.ClickRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.OptOutRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.OpenRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.BounceRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.ConversionRate)
                .HasColumnType("decimal(5,2)");

            builder.Property(a => a.ConfidenceLevel)
                .HasColumnType("decimal(5,2)");

            // Relationships
            builder.HasOne(a => a.CampaignVariant)
                .WithOne(v => v.Analytics)
                .HasForeignKey<CampaignVariantAnalytics>(a => a.CampaignVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(a => a.CampaignVariantId)
                .IsUnique();
        }
    }
}
