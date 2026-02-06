using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PrivilegedActionLogConfiguration : IEntityTypeConfiguration<PrivilegedActionLog>
    {
        public void Configure(EntityTypeBuilder<PrivilegedActionLog> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ActionType)
                .IsRequired();

            builder.Property(p => p.Severity)
                .IsRequired();

            builder.Property(p => p.PerformedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.EntityType)
                .HasMaxLength(100);

            builder.Property(p => p.EntityId)
                .HasMaxLength(450);

            builder.Property(p => p.EntityName)
                .HasMaxLength(500);

            builder.Property(p => p.ActionDescription)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(p => p.BeforeState)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.AfterState)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.IPAddress)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(p => p.UserAgent)
                .HasMaxLength(500);

            builder.Property(p => p.RequestPath)
                .HasMaxLength(500);

            builder.Property(p => p.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(p => p.Timestamp)
                .IsRequired();

            builder.Property(p => p.Metadata)
                .HasColumnType("nvarchar(max)");

            // Indexes for efficient querying
            builder.HasIndex(p => p.ActionType);
            builder.HasIndex(p => p.PerformedBy);
            builder.HasIndex(p => p.Timestamp);
            builder.HasIndex(p => p.EntityType);
            builder.HasIndex(p => p.Severity);
            builder.HasIndex(p => new { p.EntityType, p.EntityId });

            // Relationships
            builder.HasOne(p => p.PerformedByUser)
                .WithMany()
                .HasForeignKey(p => p.PerformedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
