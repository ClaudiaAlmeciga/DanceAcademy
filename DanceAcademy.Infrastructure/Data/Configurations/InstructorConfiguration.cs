#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> b)
    {
        b.ToTable("Instructors");

        b.HasKey(x => x.Id);

        b.Property(x => x.FullName)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Specialty)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Bio)
            .HasMaxLength(4000);

        b.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);
    }
}
