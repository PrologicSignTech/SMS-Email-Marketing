using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class CampaignContentConfiguration : IEntityTypeConfiguration<CampaignContent>
    {
        public void Configure(EntityTypeBuilder<CampaignContent> builder)
        {
            builder.HasKey(cc => cc.Id);

            builder.Property(cc => cc.Subject)
                .HasMaxLength(200);

            // Indexes
            builder.HasIndex(cc => cc.CampaignId);
            builder.HasIndex(cc => cc.MessageTemplateId);

            // Relationships
            builder.HasOne(cc => cc.MessageTemplate)
                .WithMany(mt => mt.CampaignContents)
                .HasForeignKey(cc => cc.MessageTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
