using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class MealPlanItemConfiguration : IEntityTypeConfiguration<MealPlanItem>
{
    public void Configure(EntityTypeBuilder<MealPlanItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.DayOfWeek)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(15);

        builder.Property(i => i.MealType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Notes)
            .HasMaxLength(500);

        builder.HasIndex(i => new { i.MealPlanId, i.DayOfWeek, i.MealType }).IsUnique();

        builder.HasOne(i => i.AssignedCook)
            .WithMany()
            .HasForeignKey(i => i.AssignedCookId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Recipe)
            .WithMany()
            .HasForeignKey(i => i.RecipeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
