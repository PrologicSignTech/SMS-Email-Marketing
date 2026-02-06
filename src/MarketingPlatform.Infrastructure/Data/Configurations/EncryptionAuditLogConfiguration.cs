using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class EncryptionAuditLogConfiguration : IEntityTypeConfiguration<EncryptionAuditLog>
    {
        public void Configure(EntityTypeBuilder<EncryptionAuditLog> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Operation)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.FieldName)
                .HasMaxLength(100);

            builder.Property(e => e.KeyVersion)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.UserId)
                .HasMaxLength(450);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(e => e.IpAddress)
                .HasMaxLength(50);

            // Indexes for querying
            builder.HasIndex(e => e.Operation);
            builder.HasIndex(e => e.EntityType);
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => e.OperationTimestamp);
            builder.HasIndex(e => e.UserId);
        }
    }
}
