#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> b)
    {
        b.ToTable("Certificates");

        b.HasKey(x => x.Id);

        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.CourseId).IsRequired();
        b.Property(x => x.IssuedAt).IsRequired();

        b.Property(x => x.VerificationCode)
            .HasMaxLength(20)
            .IsRequired();

        b.HasIndex(x => x.VerificationCode).IsUnique();

        // Un certificado por curso por estudiante
        b.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne<Course>()
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
