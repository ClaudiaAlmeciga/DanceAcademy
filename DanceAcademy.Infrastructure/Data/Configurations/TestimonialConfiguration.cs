#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> b)
    {
        b.ToTable("Testimonials");

        b.HasKey(x => x.Id);

        b.Property(x => x.UserId).IsRequired();

        b.Property(x => x.StudentName)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Content)
            .HasMaxLength(2000)
            .IsRequired();

        b.Property(x => x.Rating).IsRequired();

        b.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        b.Property(x => x.IsPublished).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        b.HasOne<Course>()
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
