using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class MessageDeliveryAttemptConfiguration : IEntityTypeConfiguration<MessageDeliveryAttempt>
    {
        public void Configure(EntityTypeBuilder<MessageDeliveryAttempt> builder)
        {
            builder.ToTable("MessageDeliveryAttempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CampaignMessageId)
                .IsRequired();

            builder.Property(x => x.AttemptNumber)
                .IsRequired();

            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.ProviderName)
                .HasMaxLength(100);

            builder.Property(x => x.AttemptedAt)
                .IsRequired();

            builder.Property(x => x.Success)
                .IsRequired();

            builder.Property(x => x.ExternalMessageId)
                .HasMaxLength(200);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(100);

            builder.Property(x => x.CostAmount)
                .HasColumnType("decimal(18,6)");

            builder.Property(x => x.ResponseTimeMs)
                .IsRequired();

            builder.Property(x => x.FallbackReason)
                .HasConversion<int?>();

            builder.Property(x => x.AdditionalMetadata)
                .HasMaxLength(4000);

            builder.HasOne(x => x.CampaignMessage)
                .WithMany()
                .HasForeignKey(x => x.CampaignMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => x.CampaignMessageId);
            builder.HasIndex(x => x.AttemptedAt);
            builder.HasIndex(x => new { x.CampaignMessageId, x.AttemptNumber });
        }
    }
}
