using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ContactTagAssignmentConfiguration : IEntityTypeConfiguration<ContactTagAssignment>
    {
        public void Configure(EntityTypeBuilder<ContactTagAssignment> builder)
        {
            builder.HasKey(cta => cta.Id);

            // Indexes
            builder.HasIndex(cta => cta.ContactId);
            builder.HasIndex(cta => cta.ContactTagId);
            builder.HasIndex(cta => new { cta.ContactId, cta.ContactTagId }).IsUnique();

            // Relationships
            builder.HasOne(cta => cta.Contact)
                .WithMany(c => c.TagAssignments)
                .HasForeignKey(cta => cta.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            // Use Restrict to avoid multiple cascade paths from ApplicationUser
            // Path 1: ApplicationUser -> Contact -> ContactTagAssignment (Cascade)
            // Path 2: ApplicationUser -> ContactTag -> ContactTagAssignment (Restrict breaks the cycle)
            builder.HasOne(cta => cta.ContactTag)
                .WithMany(ct => ct.TagAssignments)
                .HasForeignKey(cta => cta.ContactTagId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
