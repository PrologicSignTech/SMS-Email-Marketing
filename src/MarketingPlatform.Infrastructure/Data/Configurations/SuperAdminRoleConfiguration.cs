using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class SuperAdminRoleConfiguration : IEntityTypeConfiguration<SuperAdminRole>
    {
        public void Configure(EntityTypeBuilder<SuperAdminRole> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(s => s.AssignedBy)
                .HasMaxLength(450);

            builder.Property(s => s.AssignmentReason)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.RevokedBy)
                .HasMaxLength(450);

            builder.Property(s => s.RevocationReason)
                .HasMaxLength(1000);

            builder.Property(s => s.AssignedAt)
                .IsRequired();

            builder.Property(s => s.IsActive)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.IsActive);
            builder.HasIndex(s => s.AssignedAt);

            // Relationships
            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.AssignedByUser)
                .WithMany()
                .HasForeignKey(s => s.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.RevokedByUser)
                .WithMany()
                .HasForeignKey(s => s.RevokedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
