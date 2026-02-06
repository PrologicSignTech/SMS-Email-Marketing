using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class UsageTrackingConfiguration : IEntityTypeConfiguration<UsageTracking>
    {
        public void Configure(EntityTypeBuilder<UsageTracking> builder)
        {
            builder.HasKey(ut => ut.Id);

            builder.Property(ut => ut.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // Indexes
            builder.HasIndex(ut => ut.UserId);
            builder.HasIndex(ut => new { ut.UserId, ut.Year, ut.Month }).IsUnique();

            // Relationships
            builder.HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
