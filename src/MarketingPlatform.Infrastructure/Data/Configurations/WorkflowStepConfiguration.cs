using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketingPlatform.Infrastructure.Data.Configurations
{
    public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
    {
        public void Configure(EntityTypeBuilder<WorkflowStep> builder)
        {
            builder.HasKey(ws => ws.Id);

            builder.Property(ws => ws.WorkflowId)
                .IsRequired();

            builder.Property(ws => ws.StepOrder)
                .IsRequired();

            builder.Property(ws => ws.ActionType)
                .IsRequired();

            builder.Property(ws => ws.NodeLabel)
                .HasMaxLength(200);

            // Indexes
            builder.HasIndex(ws => ws.WorkflowId);
            builder.HasIndex(ws => new { ws.WorkflowId, ws.StepOrder });

            // Relationships
            builder.HasOne(ws => ws.Workflow)
                .WithMany(w => w.Steps)
                .HasForeignKey(ws => ws.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
