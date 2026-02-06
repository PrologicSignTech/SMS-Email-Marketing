using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PricingModelConfiguration : IEntityTypeConfiguration<PricingModel>
    {
        public void Configure(EntityTypeBuilder<PricingModel> builder)
        {
            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(pm => pm.Description)
                .HasMaxLength(1000);

            builder.Property(pm => pm.Type)
                .IsRequired();

            builder.Property(pm => pm.BasePrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(pm => pm.BillingPeriod)
                .IsRequired();

            builder.Property(pm => pm.Configuration)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(pm => pm.Name);
            builder.HasIndex(pm => pm.IsActive);
            builder.HasIndex(pm => pm.Priority);

            // Relationships
            builder.HasMany(pm => pm.ChannelPricings)
                .WithOne(cp => cp.PricingModel)
                .HasForeignKey(cp => cp.PricingModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pm => pm.RegionPricings)
                .WithOne(rp => rp.PricingModel)
                .HasForeignKey(rp => rp.PricingModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pm => pm.UsagePricings)
                .WithOne(up => up.PricingModel)
                .HasForeignKey(up => up.PricingModelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ChannelPricingConfiguration : IEntityTypeConfiguration<ChannelPricing>
    {
        public void Configure(EntityTypeBuilder<ChannelPricing> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.Channel)
                .IsRequired();

            builder.Property(cp => cp.PricePerUnit)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(cp => cp.MinimumCharge)
                .HasColumnType("decimal(18,2)");

            builder.Property(cp => cp.Configuration)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(cp => new { cp.PricingModelId, cp.Channel });
            builder.HasIndex(cp => cp.IsActive);
        }
    }

    public class RegionPricingConfiguration : IEntityTypeConfiguration<RegionPricing>
    {
        public void Configure(EntityTypeBuilder<RegionPricing> builder)
        {
            builder.HasKey(rp => rp.Id);

            builder.Property(rp => rp.RegionCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(rp => rp.RegionName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rp => rp.PriceMultiplier)
                .IsRequired()
                .HasColumnType("decimal(10,4)");

            builder.Property(rp => rp.FlatAdjustment)
                .HasColumnType("decimal(18,2)");

            builder.Property(rp => rp.Configuration)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(rp => new { rp.PricingModelId, rp.RegionCode });
            builder.HasIndex(rp => rp.IsActive);
        }
    }

    public class UsagePricingConfiguration : IEntityTypeConfiguration<UsagePricing>
    {
        public void Configure(EntityTypeBuilder<UsagePricing> builder)
        {
            builder.HasKey(up => up.Id);

            builder.Property(up => up.Type)
                .IsRequired();

            builder.Property(up => up.TierStart)
                .IsRequired();

            builder.Property(up => up.PricePerUnit)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(up => up.Configuration)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(up => new { up.PricingModelId, up.Type, up.TierStart });
            builder.HasIndex(up => up.IsActive);
        }
    }

    public class TaxConfigurationConfiguration : IEntityTypeConfiguration<TaxConfiguration>
    {
        public void Configure(EntityTypeBuilder<TaxConfiguration> builder)
        {
            builder.HasKey(tc => tc.Id);

            builder.Property(tc => tc.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(tc => tc.Type)
                .IsRequired();

            builder.Property(tc => tc.RegionCode)
                .HasMaxLength(10);

            builder.Property(tc => tc.Rate)
                .IsRequired()
                .HasColumnType("decimal(10,4)");

            builder.Property(tc => tc.FlatAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(tc => tc.Configuration)
                .HasColumnType("nvarchar(max)");

            // Indexes
            builder.HasIndex(tc => tc.Name);
            builder.HasIndex(tc => tc.Type);
            builder.HasIndex(tc => tc.RegionCode);
            builder.HasIndex(tc => tc.IsActive);
            builder.HasIndex(tc => tc.Priority);
        }
    }
}
