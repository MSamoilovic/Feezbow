using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Feezbow.Domain.Entities;

namespace Feezbow.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Body)
            .HasMaxLength(1000);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
