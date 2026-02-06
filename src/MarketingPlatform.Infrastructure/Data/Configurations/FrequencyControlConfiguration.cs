using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class FrequencyControlConfiguration : IEntityTypeConfiguration<FrequencyControl>
    {
        public void Configure(EntityTypeBuilder<FrequencyControl> builder)
        {
            builder.HasKey(fc => fc.Id);

            builder.Property(fc => fc.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(fc => fc.ContactId)
                .IsRequired();

            builder.Property(fc => fc.MaxMessagesPerDay)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(fc => fc.MaxMessagesPerWeek)
                .IsRequired()
                .HasDefaultValue(20);

            builder.Property(fc => fc.MaxMessagesPerMonth)
                .IsRequired()
                .HasDefaultValue(50);

            builder.Property(fc => fc.MessagesSentToday)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(fc => fc.MessagesSentThisWeek)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(fc => fc.MessagesSentThisMonth)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(fc => fc.LastMessageSentAt)
                .IsRequired(false); // Allow null for new frequency controls

            // Indexes for performance
            builder.HasIndex(fc => fc.ContactId);
            builder.HasIndex(fc => fc.UserId);
            builder.HasIndex(fc => new { fc.ContactId, fc.UserId }).IsUnique();

            // Relationships
            builder.HasOne(fc => fc.Contact)
                .WithMany()
                .HasForeignKey(fc => fc.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(fc => fc.User)
                .WithMany()
                .HasForeignKey(fc => fc.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid circular cascade
        }
    }
}
