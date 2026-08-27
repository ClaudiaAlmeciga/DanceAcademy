#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("Events");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(4000);

        b.Property(x => x.Location)
            .HasMaxLength(300);

        b.Property(x => x.EventDate).IsRequired();

        b.Property(x => x.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        b.Property(x => x.Capacity).IsRequired();

        b.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        b.Property(x => x.IsPublished).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);
    }
}
