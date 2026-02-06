using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class RateLimitLogConfiguration : IEntityTypeConfiguration<RateLimitLog>
    {
        public void Configure(EntityTypeBuilder<RateLimitLog> builder)
        {
            builder.HasKey(rll => rll.Id);

            builder.Property(rll => rll.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(rll => rll.TenantId)
                .HasMaxLength(450);

            builder.Property(rll => rll.Endpoint)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rll => rll.HttpMethod)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(rll => rll.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(rll => rll.RateLimitRule)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rll => rll.RequestCount)
                .IsRequired();

            builder.Property(rll => rll.MaxRequests)
                .IsRequired();

            builder.Property(rll => rll.TimeWindowSeconds)
                .IsRequired();

            builder.Property(rll => rll.TriggeredAt)
                .IsRequired();

            builder.Property(rll => rll.RetryAfterSeconds)
                .IsRequired();

            // Indexes for performance and querying
            builder.HasIndex(rll => rll.UserId);
            builder.HasIndex(rll => rll.TenantId);
            builder.HasIndex(rll => rll.TriggeredAt);
            builder.HasIndex(rll => new { rll.UserId, rll.TriggeredAt });

            // Relationships
            builder.HasOne(rll => rll.User)
                .WithMany()
                .HasForeignKey(rll => rll.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
