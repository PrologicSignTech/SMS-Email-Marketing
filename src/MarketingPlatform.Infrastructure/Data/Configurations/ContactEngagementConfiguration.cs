using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ContactEngagementConfiguration : IEntityTypeConfiguration<ContactEngagement>
    {
        public void Configure(EntityTypeBuilder<ContactEngagement> builder)
        {
            builder.HasKey(ce => ce.Id);

            builder.Property(ce => ce.EngagementScore)
                .HasPrecision(18, 2);

            // Indexes
            builder.HasIndex(ce => ce.ContactId);
        }
    }
}
