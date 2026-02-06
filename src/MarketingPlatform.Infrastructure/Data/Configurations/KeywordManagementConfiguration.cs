using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class KeywordReservationConfiguration : IEntityTypeConfiguration<KeywordReservation>
    {
        public void Configure(EntityTypeBuilder<KeywordReservation> builder)
        {
            builder.HasKey(kr => kr.Id);

            builder.Property(kr => kr.KeywordText)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(kr => kr.RequestedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(kr => kr.ApprovedByUserId)
                .HasMaxLength(450);

            builder.Property(kr => kr.Purpose)
                .HasMaxLength(500);

            builder.Property(kr => kr.RejectionReason)
                .HasMaxLength(500);

            builder.Property(kr => kr.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(kr => kr.KeywordText);
            builder.HasIndex(kr => kr.RequestedByUserId);
            builder.HasIndex(kr => kr.Status);

            // Relationships
            builder.HasOne(kr => kr.RequestedBy)
                .WithMany()
                .HasForeignKey(kr => kr.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(kr => kr.ApprovedBy)
                .WithMany()
                .HasForeignKey(kr => kr.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class KeywordAssignmentConfiguration : IEntityTypeConfiguration<KeywordAssignment>
    {
        public void Configure(EntityTypeBuilder<KeywordAssignment> builder)
        {
            builder.HasKey(ka => ka.Id);

            builder.Property(ka => ka.AssignedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ka => ka.Notes)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(ka => ka.KeywordId);
            builder.HasIndex(ka => ka.CampaignId);
            builder.HasIndex(ka => ka.IsActive);

            // Relationships
            builder.HasOne(ka => ka.Keyword)
                .WithMany()
                .HasForeignKey(ka => ka.KeywordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Use Restrict on Campaign to avoid multiple cascade paths from ApplicationUser
            // Path 1: ApplicationUser -> Keyword -> KeywordAssignment (Cascade)
            // Path 2: ApplicationUser -> Campaign -> KeywordAssignment (Restrict breaks the cycle)
            builder.HasOne(ka => ka.Campaign)
                .WithMany()
                .HasForeignKey(ka => ka.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ka => ka.AssignedBy)
                .WithMany()
                .HasForeignKey(ka => ka.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class KeywordConflictConfiguration : IEntityTypeConfiguration<KeywordConflict>
    {
        public void Configure(EntityTypeBuilder<KeywordConflict> builder)
        {
            builder.HasKey(kc => kc.Id);

            builder.Property(kc => kc.KeywordText)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(kc => kc.RequestingUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(kc => kc.ExistingUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(kc => kc.ResolvedByUserId)
                .HasMaxLength(450);

            builder.Property(kc => kc.Resolution)
                .HasMaxLength(1000);

            builder.Property(kc => kc.Notes)
                .HasMaxLength(1000);

            builder.Property(kc => kc.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(kc => kc.KeywordText);
            builder.HasIndex(kc => kc.Status);

            // Relationships
            builder.HasOne(kc => kc.RequestingUser)
                .WithMany()
                .HasForeignKey(kc => kc.RequestingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(kc => kc.ExistingUser)
                .WithMany()
                .HasForeignKey(kc => kc.ExistingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(kc => kc.ResolvedBy)
                .WithMany()
                .HasForeignKey(kc => kc.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
