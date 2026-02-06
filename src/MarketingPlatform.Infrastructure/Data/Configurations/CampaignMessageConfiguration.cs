using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignMessageConfiguration : IEntityTypeConfiguration<CampaignMessage>
    {
        public void Configure(EntityTypeBuilder<CampaignMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.Status)
                .IsRequired();

            builder.Property(cm => cm.ProviderMessageId)
                .HasMaxLength(100);

            builder.Property(cm => cm.ErrorMessage)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(cm => cm.CampaignId);
            builder.HasIndex(cm => cm.ContactId);
            builder.HasIndex(cm => cm.Status);
            builder.HasIndex(cm => cm.VariantId);

            // Relationships
            builder.HasOne(cm => cm.Campaign)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Contact)
                .WithMany(c => c.CampaignMessages)
                .HasForeignKey(cm => cm.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cm => cm.Variant)
                .WithMany(v => v.Messages)
                .HasForeignKey(cm => cm.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
