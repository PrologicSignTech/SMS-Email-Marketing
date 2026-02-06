using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PlatformConfigurationConfiguration : IEntityTypeConfiguration<PlatformConfiguration>
    {
        public void Configure(EntityTypeBuilder<PlatformConfiguration> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Key)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Value)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(p => p.Category)
                .IsRequired();

            builder.Property(p => p.DataType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.IsEncrypted)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.Property(p => p.LastModifiedBy)
                .HasMaxLength(450);

            // Indexes
            builder.HasIndex(p => p.Key)
                .IsUnique();

            builder.HasIndex(p => p.Category);
            builder.HasIndex(p => p.IsActive);

            // Relationships
            builder.HasOne(p => p.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(p => p.LastModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
