using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ContactConfiguration : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .HasMaxLength(256);

            builder.Property(c => c.FirstName)
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .HasMaxLength(100);

            builder.Property(c => c.Country)
                .HasMaxLength(100);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.PostalCode)
                .HasMaxLength(20);

            // Indexes
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.Email);
            builder.HasIndex(c => c.PhoneNumber);
            builder.HasIndex(c => c.CreatedAt);

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Engagement)
                .WithOne(e => e.Contact)
                .HasForeignKey<ContactEngagement>(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
