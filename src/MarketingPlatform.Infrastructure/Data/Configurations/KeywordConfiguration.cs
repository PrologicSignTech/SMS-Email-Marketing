using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class KeywordConfiguration : IEntityTypeConfiguration<Keyword>
    {
        public void Configure(EntityTypeBuilder<Keyword> builder)
        {
            builder.HasKey(k => k.Id);

            builder.Property(k => k.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(k => k.KeywordText)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(k => k.Description)
                .HasMaxLength(500);

            builder.Property(k => k.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(k => k.UserId);
            builder.HasIndex(k => k.KeywordText);

            // Relationships
            builder.HasOne(k => k.User)
                .WithMany()
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(k => k.LinkedCampaign)
                .WithMany()
                .HasForeignKey(k => k.LinkedCampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(k => k.OptInGroup)
                .WithMany(cg => cg.Keywords)
                .HasForeignKey(k => k.OptInGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
