using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MarketingPlatform.Core.Entities;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PageContentConfiguration : IEntityTypeConfiguration<PageContent>
    {
        public void Configure(EntityTypeBuilder<PageContent> builder)
        {
            builder.HasOne(p => p.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(p => p.LastModifiedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(p => p.PageKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(p => p.PageKey)
                .IsUnique();
        }
    }
}