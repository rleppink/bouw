using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("workflow_actions");

        builder.HasKey(action => action.Id);
        builder.Property(action => action.Id).ValueGeneratedNever();
        builder.Property(action => action.Key).HasMaxLength(80).IsRequired();
        builder
            .Property(action => action.Type)
            .HasConversion(WorkflowValueConverters.WorkflowActionType)
            .HasMaxLength(40);
        builder.Property(action => action.Position).IsRequired();
        builder.Property(action => action.ConfigJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(action => new { action.StepId, action.Key }).IsUnique();
        builder.HasIndex(action => new { action.StepId, action.Position }).IsUnique();
    }
}
