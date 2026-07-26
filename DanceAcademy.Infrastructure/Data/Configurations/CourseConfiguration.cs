#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("Courses");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(2000);

        b.Property(x => x.LevelId).IsRequired();

        b.Property(x => x.PricingType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        b.Property(x => x.Price)
            .HasColumnType("decimal(10,2)");

        b.Property(x => x.IsPublished).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        // Índice para búsquedas básicas
        b.HasIndex(x => x.Title);

        b.HasOne<Level>()
            .WithMany()
            .HasForeignKey(x => x.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación 1..N
        b.HasMany(x => x.Modules)
            .WithOne()
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación N..N — un curso puede estar incluido en varios planes
        b.HasMany(x => x.SubscriptionPlans)
            .WithMany()
            .UsingEntity(j => j.ToTable("CourseSubscriptionPlans"));
    }
}
