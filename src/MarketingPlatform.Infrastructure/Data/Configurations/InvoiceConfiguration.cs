using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.Status)
                .IsRequired();

            builder.Property(i => i.Amount)
                .HasPrecision(18, 2);

            builder.Property(i => i.Tax)
                .HasPrecision(18, 2);

            builder.Property(i => i.Total)
                .HasPrecision(18, 2);

            builder.Property(i => i.StripeInvoiceId)
                .HasMaxLength(100);

            builder.Property(i => i.Description)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(i => i.UserId);
            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.HasIndex(i => i.Status);

            // Relationships
            builder.HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.UserSubscription)
                .WithMany(us => us.Invoices)
                .HasForeignKey(i => i.UserSubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
