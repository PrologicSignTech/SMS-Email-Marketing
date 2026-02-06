using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class URLShortenerConfiguration : IEntityTypeConfiguration<URLShortener>
    {
        public void Configure(EntityTypeBuilder<URLShortener> builder)
        {
            builder.HasKey(us => us.Id);

            builder.Property(us => us.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(us => us.ShortCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(us => us.ShortUrl)
                .IsRequired()
                .HasMaxLength(200);

            // Indexes
            builder.HasIndex(us => us.CampaignId);
            builder.HasIndex(us => us.ShortCode).IsUnique();

            // Relationships
            builder.HasOne(us => us.Campaign)
                .WithMany(c => c.URLShorteners)
                .HasForeignKey(us => us.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
