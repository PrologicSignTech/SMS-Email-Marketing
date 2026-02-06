using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
    {
        public void Configure(EntityTypeBuilder<MessageTemplate> builder)
        {
            builder.HasKey(mt => mt.Id);

            builder.Property(mt => mt.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(mt => mt.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(mt => mt.Channel)
                .IsRequired();

            builder.Property(mt => mt.Category)
                .HasMaxLength(100);

            builder.Property(mt => mt.Subject)
                .HasMaxLength(200);

            builder.Property(mt => mt.MessageBody)
                .IsRequired();

            // Indexes
            builder.HasIndex(mt => mt.UserId);
            builder.HasIndex(mt => mt.Channel);
            builder.HasIndex(mt => mt.Category);

            // Relationships
            builder.HasOne(mt => mt.User)
                .WithMany()
                .HasForeignKey(mt => mt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
