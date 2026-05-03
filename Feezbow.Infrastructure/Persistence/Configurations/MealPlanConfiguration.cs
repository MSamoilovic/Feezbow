using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.WeekStart)
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasIndex(p => new { p.ProjectId, p.WeekStart }).IsUnique();

        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(i => i.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(MealPlan.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
