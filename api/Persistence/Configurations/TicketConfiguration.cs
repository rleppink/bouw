using Bouw.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bouw.API.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tickets");

        builder.HasKey(ticket => ticket.Id);
        builder.Property(ticket => ticket.Id).ValueGeneratedNever();
        builder.Property(ticket => ticket.Title).HasMaxLength(80).IsRequired();
        builder.Property(ticket => ticket.UserInput).HasColumnType("text").IsRequired();
        builder
            .Property(ticket => ticket.Status)
            .HasConversion(WorkflowValueConverters.TicketStatus)
            .HasMaxLength(40);
        builder.Property(ticket => ticket.LlmResponse).HasColumnType("text").IsRequired();
        builder.Property(ticket => ticket.CreatedAt).IsRequired();
        builder.Property(ticket => ticket.UpdatedAt).IsRequired();

        builder.HasIndex(ticket => ticket.CreatedAt);
    }
}
