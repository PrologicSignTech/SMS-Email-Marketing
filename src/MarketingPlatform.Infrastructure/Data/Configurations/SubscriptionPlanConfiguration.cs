using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sp => sp.Description)
                .HasMaxLength(500);

            builder.Property(sp => sp.PlanCategory)
                .HasMaxLength(200);

            builder.Property(sp => sp.PriceMonthly)
                .HasPrecision(18, 2);

            builder.Property(sp => sp.PriceYearly)
                .HasPrecision(18, 2);

            builder.Property(sp => sp.StripeProductId)
                .HasMaxLength(100);

            builder.Property(sp => sp.StripePriceIdMonthly)
                .HasMaxLength(100);

            builder.Property(sp => sp.StripePriceIdYearly)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(sp => sp.Name);
        }
    }
}
