#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.ToTable("Lessons");

        b.HasKey(x => x.Id);

        b.Property(x => x.ModuleId).IsRequired();

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Order).IsRequired();

        b.Property(x => x.Content)
            .HasMaxLength(8000);

        b.Property(x => x.VideoUrl)
            .HasMaxLength(2000);

        b.Property(x => x.IsPublished).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        // Evita órdenes duplicados dentro del mismo módulo
        b.HasIndex(x => new { x.ModuleId, x.Order })
            .IsUnique();
    }
}
