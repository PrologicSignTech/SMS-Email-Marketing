using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ChannelRoutingConfigConfiguration : IEntityTypeConfiguration<ChannelRoutingConfig>
    {
        public void Configure(EntityTypeBuilder<ChannelRoutingConfig> builder)
        {
            builder.ToTable("ChannelRoutingConfigs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.PrimaryProvider)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FallbackProvider)
                .HasMaxLength(100);

            builder.Property(x => x.RoutingStrategy)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.EnableFallback)
                .IsRequired();

            builder.Property(x => x.MaxRetries)
                .IsRequired();

            builder.Property(x => x.RetryStrategy)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.InitialRetryDelaySeconds)
                .IsRequired();

            builder.Property(x => x.MaxRetryDelaySeconds)
                .IsRequired();

            builder.Property(x => x.CostThreshold)
                .HasColumnType("decimal(18,6)");

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.Priority)
                .IsRequired();

            builder.Property(x => x.AdditionalSettings)
                .HasMaxLength(4000);

            // Indexes for performance
            builder.HasIndex(x => x.Channel);
            builder.HasIndex(x => new { x.Channel, x.IsActive, x.Priority });
        }
    }
}
