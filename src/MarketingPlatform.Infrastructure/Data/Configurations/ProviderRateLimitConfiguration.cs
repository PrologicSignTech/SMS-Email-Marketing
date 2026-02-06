using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ProviderRateLimitConfiguration : IEntityTypeConfiguration<ProviderRateLimit>
    {
        public void Configure(EntityTypeBuilder<ProviderRateLimit> builder)
        {
            builder.HasKey(prl => prl.Id);

            builder.Property(prl => prl.ProviderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(prl => prl.ProviderType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(prl => prl.MaxRequests)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(prl => prl.TimeWindowSeconds)
                .IsRequired()
                .HasDefaultValue(60);

            builder.Property(prl => prl.CurrentRequestCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(prl => prl.WindowStartTime)
                .IsRequired();

            builder.Property(prl => prl.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(prl => prl.UserId)
                .HasMaxLength(450);

            // Indexes for performance
            builder.HasIndex(prl => prl.ProviderName);
            builder.HasIndex(prl => prl.ProviderType);
            builder.HasIndex(prl => new { prl.ProviderName, prl.ProviderType });
            builder.HasIndex(prl => new { prl.UserId, prl.ProviderName });
        }
    }
}
