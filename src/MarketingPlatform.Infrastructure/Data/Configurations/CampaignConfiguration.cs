using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.Type)
                .IsRequired();

            builder.Property(c => c.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.CreatedAt);
            builder.HasIndex(c => c.ScheduledAt);

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany(u => u.Campaigns)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Content)
                .WithOne(cc => cc.Campaign)
                .HasForeignKey<CampaignContent>(cc => cc.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Audience)
                .WithOne(ca => ca.Campaign)
                .HasForeignKey<CampaignAudience>(ca => ca.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Schedule)
                .WithOne(cs => cs.Campaign)
                .HasForeignKey<CampaignSchedule>(cs => cs.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Analytics)
                .WithOne(ca => ca.Campaign)
                .HasForeignKey<CampaignAnalytics>(ca => ca.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
