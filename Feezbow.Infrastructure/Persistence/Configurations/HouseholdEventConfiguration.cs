using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class HouseholdEventConfiguration : IEntityTypeConfiguration<HouseholdEvent>
{
    public void Configure(EntityTypeBuilder<HouseholdEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.IsAllDay)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Category)
            .HasMaxLength(50);

        builder.Property(e => e.Color)
            .HasMaxLength(20);

        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => new { e.ProjectId, e.StartDate });

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.AssignedTo)
            .WithMany()
            .HasForeignKey(e => e.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
