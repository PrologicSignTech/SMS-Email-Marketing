using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ChatRoomId)
                .IsRequired();

            builder.Property(c => c.SenderId)
                .HasMaxLength(450);

            builder.Property(c => c.MessageText)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.IsRead)
                .IsRequired();

            builder.Property(c => c.SentAt)
                .IsRequired();

            builder.Property(c => c.MessageType)
                .IsRequired();

            builder.Property(c => c.AttachmentUrl)
                .HasMaxLength(500);

            builder.Property(c => c.AttachmentFileName)
                .HasMaxLength(255);

            // Indexes
            builder.HasIndex(c => c.ChatRoomId);
            builder.HasIndex(c => c.SenderId);
            builder.HasIndex(c => c.SentAt);
            builder.HasIndex(c => c.IsRead);

            // Relationships
            builder.HasOne(c => c.ChatRoom)
                .WithMany(r => r.Messages)
                .HasForeignKey(c => c.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Sender)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
