using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ContactGroupConfiguration : IEntityTypeConfiguration<ContactGroup>
    {
        public void Configure(EntityTypeBuilder<ContactGroup> builder)
        {
            builder.HasKey(cg => cg.Id);

            builder.Property(cg => cg.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cg => cg.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(cg => cg.Description)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(cg => cg.UserId);
            builder.HasIndex(cg => cg.Name);

            // Relationships
            builder.HasOne(cg => cg.User)
                .WithMany(u => u.ContactGroups)
                .HasForeignKey(cg => cg.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
