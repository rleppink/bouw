using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class WorkflowSessionConfiguration : IEntityTypeConfiguration<WorkflowSession>
{
    public void Configure(EntityTypeBuilder<WorkflowSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("workflow_sessions");

        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedNever();
        builder.Property(session => session.Title).HasMaxLength(200).IsRequired();
        builder
            .Property(session => session.Status)
            .HasConversion(WorkflowValueConverters.SessionStatus)
            .HasMaxLength(40);
        builder.Property(session => session.CurrentStepKey).HasMaxLength(80);
        builder.Property(session => session.CreatedAt).IsRequired();
        builder.Property(session => session.UpdatedAt).IsRequired();

        builder.HasIndex(session => session.WorkflowId);
        builder.HasIndex(session => session.Status);

        builder
            .HasOne(session => session.Workflow)
            .WithMany()
            .HasForeignKey(session => session.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(session => session.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder
            .Navigation(session => session.ActionRuns)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder
            .Navigation(session => session.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
