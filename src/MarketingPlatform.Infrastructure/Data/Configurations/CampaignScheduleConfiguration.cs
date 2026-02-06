using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignScheduleConfiguration : IEntityTypeConfiguration<CampaignSchedule>
    {
        public void Configure(EntityTypeBuilder<CampaignSchedule> builder)
        {
            builder.HasKey(cs => cs.Id);

            builder.Property(cs => cs.ScheduleType)
                .IsRequired();

            builder.Property(cs => cs.TimeZone)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(cs => cs.CampaignId);
        }
    }
}
