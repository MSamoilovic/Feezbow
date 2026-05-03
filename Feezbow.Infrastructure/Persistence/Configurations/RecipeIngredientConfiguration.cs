using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasPrecision(12, 3);

        builder.Property(i => i.Unit)
            .HasMaxLength(30);

        builder.Property(i => i.Notes)
            .HasMaxLength(300);

        builder.HasIndex(i => new { i.RecipeId, i.OrderIndex });
    }
}
