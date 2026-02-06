using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(r => r.Description)
                .HasMaxLength(500);
            
            builder.Property(r => r.Permissions)
                .IsRequired();
            
            builder.Property(r => r.IsSystemRole)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(r => r.CreatedAt)
                .IsRequired();
            
            builder.HasIndex(r => r.Name)
                .IsUnique();
            
            builder.HasIndex(r => r.IsActive);
        }
    }
}
