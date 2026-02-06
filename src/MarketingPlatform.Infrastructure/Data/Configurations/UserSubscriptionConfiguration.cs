using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.HasKey(us => us.Id);

            builder.Property(us => us.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(us => us.Status)
                .IsRequired();

            builder.Property(us => us.StripeSubscriptionId)
                .HasMaxLength(100);

            builder.Property(us => us.StripeCustomerId)
                .HasMaxLength(100);

            builder.Property(us => us.CancellationReason)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(us => us.UserId);
            builder.HasIndex(us => us.Status);

            // Relationships
            builder.HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(us => us.SubscriptionPlan)
                .WithMany(sp => sp.UserSubscriptions)
                .HasForeignKey(us => us.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
