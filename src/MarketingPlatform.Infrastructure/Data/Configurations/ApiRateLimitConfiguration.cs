using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ApiRateLimitConfiguration : IEntityTypeConfiguration<ApiRateLimit>
    {
        public void Configure(EntityTypeBuilder<ApiRateLimit> builder)
        {
            builder.HasKey(arl => arl.Id);

            builder.Property(arl => arl.UserId)
                .HasMaxLength(450);

            builder.Property(arl => arl.TenantId)
                .HasMaxLength(450);

            builder.Property(arl => arl.EndpointPattern)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(arl => arl.MaxRequests)
                .IsRequired()
                .HasDefaultValue(100);

            builder.Property(arl => arl.TimeWindowSeconds)
                .IsRequired()
                .HasDefaultValue(60);

            builder.Property(arl => arl.CurrentRequestCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(arl => arl.WindowStartTime)
                .IsRequired();

            builder.Property(arl => arl.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(arl => arl.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes for performance
            builder.HasIndex(arl => arl.UserId);
            builder.HasIndex(arl => arl.TenantId);
            builder.HasIndex(arl => arl.EndpointPattern);
            builder.HasIndex(arl => new { arl.UserId, arl.EndpointPattern });
            builder.HasIndex(arl => new { arl.TenantId, arl.EndpointPattern });

            // Relationships
            builder.HasOne(arl => arl.User)
                .WithMany()
                .HasForeignKey(arl => arl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
