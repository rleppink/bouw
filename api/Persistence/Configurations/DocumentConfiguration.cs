using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("documents");

        builder.HasKey(document => document.Id);
        builder.Property(document => document.Id).ValueGeneratedNever();
        builder.Property(document => document.Kind).HasMaxLength(80).IsRequired();
        builder.Property(document => document.Title).HasMaxLength(200).IsRequired();
        builder.Property(document => document.ContentMarkdown).IsRequired();
        builder.Property(document => document.Version).IsRequired();
        builder.Property(document => document.CreatedBy).HasMaxLength(120).IsRequired();
        builder.Property(document => document.CreatedAt).IsRequired();
        builder.Property(document => document.UpdatedAt).IsRequired();

        builder.HasIndex(document => new { document.SessionId, document.Kind }).IsUnique();

        builder
            .HasOne(document => document.WorkflowSession)
            .WithMany(session => session.Documents)
            .HasForeignKey(document => document.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
