using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignVariantConfiguration : IEntityTypeConfiguration<CampaignVariant>
    {
        public void Configure(EntityTypeBuilder<CampaignVariant> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(v => v.Description)
                .HasMaxLength(1000);

            builder.Property(v => v.TrafficPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(v => v.Subject)
                .HasMaxLength(500);

            builder.Property(v => v.MessageBody)
                .HasColumnType("nvarchar(max)");

            builder.Property(v => v.HTMLContent)
                .HasColumnType("nvarchar(max)");

            builder.Property(v => v.MediaUrls)
                .HasColumnType("nvarchar(max)");

            builder.Property(v => v.PersonalizationTokens)
                .HasColumnType("nvarchar(max)");

            // Relationships
            builder.HasOne(v => v.Campaign)
                .WithMany(c => c.Variants)
                .HasForeignKey(v => v.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.MessageTemplate)
                .WithMany()
                .HasForeignKey(v => v.MessageTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(v => v.Analytics)
                .WithOne(a => a.CampaignVariant)
                .HasForeignKey<CampaignVariantAnalytics>(a => a.CampaignVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(v => v.CampaignId);
            builder.HasIndex(v => new { v.CampaignId, v.IsControl });
        }
    }
}
