using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
    {
        public void Configure(EntityTypeBuilder<PlatformSetting> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.Key)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ps => ps.Value)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(ps => ps.Description)
                .HasMaxLength(1000);

            builder.Property(ps => ps.Category)
                .HasMaxLength(100);

            builder.Property(ps => ps.DefaultValue)
                .HasMaxLength(4000);

            builder.Property(ps => ps.ModifiedBy)
                .HasMaxLength(450);

            builder.Property(ps => ps.DataType)
                .IsRequired();

            builder.Property(ps => ps.Scope)
                .IsRequired();

            // Indexes
            builder.HasIndex(ps => ps.Key)
                .IsUnique();

            builder.HasIndex(ps => ps.Category);
            builder.HasIndex(ps => ps.Scope);
            builder.HasIndex(ps => new { ps.IsDeleted, ps.Category });
        }
    }

    public class FeatureToggleConfiguration : IEntityTypeConfiguration<FeatureToggle>
    {
        public void Configure(EntityTypeBuilder<FeatureToggle> builder)
        {
            builder.HasKey(ft => ft.Id);

            builder.Property(ft => ft.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ft => ft.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(ft => ft.Description)
                .HasMaxLength(1000);

            builder.Property(ft => ft.Category)
                .HasMaxLength(100);

            builder.Property(ft => ft.EnabledForRoles)
                .HasMaxLength(500);

            builder.Property(ft => ft.EnabledForUsers)
                .HasMaxLength(2000);

            builder.Property(ft => ft.ModifiedBy)
                .HasMaxLength(450);

            builder.Property(ft => ft.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(ft => ft.Name)
                .IsUnique();

            builder.HasIndex(ft => ft.Category);
            builder.HasIndex(ft => ft.Status);
            builder.HasIndex(ft => new { ft.IsDeleted, ft.IsEnabled });
        }
    }

    public class ComplianceRuleConfiguration : IEntityTypeConfiguration<ComplianceRule>
    {
        public void Configure(EntityTypeBuilder<ComplianceRule> builder)
        {
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(cr => cr.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(cr => cr.Configuration)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(cr => cr.ApplicableRegions)
                .HasMaxLength(1000);

            builder.Property(cr => cr.ApplicableServices)
                .HasMaxLength(1000);

            builder.Property(cr => cr.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(cr => cr.ModifiedBy)
                .HasMaxLength(450);

            builder.Property(cr => cr.RuleType)
                .IsRequired();

            builder.Property(cr => cr.Status)
                .IsRequired();

            // Indexes
            builder.HasIndex(cr => cr.RuleType);
            builder.HasIndex(cr => cr.Status);
            builder.HasIndex(cr => cr.Priority);
            builder.HasIndex(cr => new { cr.IsDeleted, cr.Status });
            builder.HasIndex(cr => new { cr.Status, cr.EffectiveFrom, cr.EffectiveTo });

            // Relationships
            builder.HasMany(cr => cr.AuditTrail)
                .WithOne(a => a.ComplianceRule)
                .HasForeignKey(a => a.ComplianceRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ComplianceRuleAuditConfiguration : IEntityTypeConfiguration<ComplianceRuleAudit>
    {
        public void Configure(EntityTypeBuilder<ComplianceRuleAudit> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.PerformedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(a => a.PreviousState)
                .HasColumnType("nvarchar(max)");

            builder.Property(a => a.NewState)
                .HasColumnType("nvarchar(max)");

            builder.Property(a => a.Reason)
                .HasMaxLength(1000);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(50);

            builder.Property(a => a.Metadata)
                .HasColumnType("nvarchar(max)");

            builder.Property(a => a.Action)
                .IsRequired();

            // Indexes
            builder.HasIndex(a => a.ComplianceRuleId);
            builder.HasIndex(a => a.Action);
            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => new { a.ComplianceRuleId, a.CreatedAt });
        }
    }
}
