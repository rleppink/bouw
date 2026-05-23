using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("workflows");

        builder.HasKey(workflow => workflow.Id);
        builder.Property(workflow => workflow.Id).ValueGeneratedNever();
        builder.Property(workflow => workflow.Key).HasMaxLength(80).IsRequired();
        builder.Property(workflow => workflow.Name).HasMaxLength(160).IsRequired();
        builder.Property(workflow => workflow.Description).HasMaxLength(1000).IsRequired();
        builder
            .Property(workflow => workflow.Status)
            .HasConversion(WorkflowValueConverters.WorkflowStatus)
            .HasMaxLength(40);

        builder.HasIndex(workflow => workflow.Key).IsUnique();

        builder
            .HasMany(workflow => workflow.Steps)
            .WithOne(step => step.Workflow)
            .HasForeignKey(step => step.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(workflow => workflow.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
