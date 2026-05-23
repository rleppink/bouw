using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("workflow_steps");

        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id).ValueGeneratedNever();
        builder.Property(step => step.Key).HasMaxLength(80).IsRequired();
        builder.Property(step => step.Name).HasMaxLength(160).IsRequired();
        builder.Property(step => step.Position).IsRequired();

        builder.HasIndex(step => new { step.WorkflowId, step.Key }).IsUnique();
        builder.HasIndex(step => new { step.WorkflowId, step.Position }).IsUnique();

        builder
            .HasMany(step => step.Actions)
            .WithOne(action => action.WorkflowStep)
            .HasForeignKey(action => action.StepId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(step => step.Actions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
