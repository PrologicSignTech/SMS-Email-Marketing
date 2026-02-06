using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignAudienceConfiguration : IEntityTypeConfiguration<CampaignAudience>
    {
        public void Configure(EntityTypeBuilder<CampaignAudience> builder)
        {
            builder.HasKey(ca => ca.Id);

            builder.Property(ca => ca.TargetType)
                .IsRequired();

            // Indexes
            builder.HasIndex(ca => ca.CampaignId);
        }
    }
}
