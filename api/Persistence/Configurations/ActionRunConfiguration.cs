using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class ActionRunConfiguration : IEntityTypeConfiguration<ActionRun>
{
    public void Configure(EntityTypeBuilder<ActionRun> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("action_runs");

        builder.HasKey(run => run.Id);
        builder.Property(run => run.Id).ValueGeneratedNever();
        builder.Property(run => run.StepKey).HasMaxLength(80).IsRequired();
        builder.Property(run => run.ActionKey).HasMaxLength(80).IsRequired();
        builder
            .Property(run => run.Status)
            .HasConversion(WorkflowValueConverters.ActionRunStatus)
            .HasMaxLength(40);
        builder.Property(run => run.InputJson).HasColumnType("jsonb").IsRequired();
        builder.Property(run => run.OutputJson).HasColumnType("jsonb");
        builder.Property(run => run.Error).HasMaxLength(4000);
        builder.Property(run => run.StartedAt).IsRequired();

        builder.HasIndex(run => new
        {
            run.SessionId,
            run.StepKey,
            run.ActionKey,
        });

        builder
            .HasOne(run => run.WorkflowSession)
            .WithMany(session => session.ActionRuns)
            .HasForeignKey(run => run.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
