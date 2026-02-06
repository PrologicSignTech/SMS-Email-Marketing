using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.GuestName)
                .HasMaxLength(200);

            builder.Property(c => c.GuestEmail)
                .HasMaxLength(256);

            builder.Property(c => c.CustomerId)
                .HasMaxLength(450);

            builder.Property(c => c.AssignedEmployeeId)
                .HasMaxLength(450);

            builder.Property(c => c.Status)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(c => c.CustomerId);
            builder.HasIndex(c => c.AssignedEmployeeId);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.CreatedAt);
            builder.HasIndex(c => c.GuestEmail);

            // Relationships
            builder.HasOne(c => c.Customer)
                .WithMany(u => u.CustomerChatRooms)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.AssignedEmployee)
                .WithMany(u => u.AssignedChatRooms)
                .HasForeignKey(c => c.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
