using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class SessionStepConfiguration : IEntityTypeConfiguration<SessionStep>
{
    public void Configure(EntityTypeBuilder<SessionStep> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("session_steps");

        builder.HasKey(step => step.Id);
        builder.Property(step => step.Id).ValueGeneratedNever();
        builder.Property(step => step.StepKey).HasMaxLength(80).IsRequired();
        builder
            .Property(step => step.Status)
            .HasConversion(WorkflowValueConverters.SessionStepStatus)
            .HasMaxLength(40);

        builder.HasIndex(step => new { step.SessionId, step.StepKey }).IsUnique();

        builder
            .HasOne(step => step.WorkflowSession)
            .WithMany(session => session.Steps)
            .HasForeignKey(step => step.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
