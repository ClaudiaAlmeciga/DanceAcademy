#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class NewsPostConfiguration : IEntityTypeConfiguration<NewsPost>
{
    public void Configure(EntityTypeBuilder<NewsPost> b)
    {
        b.ToTable("NewsPosts");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Content)
            .HasMaxLength(8000)
            .IsRequired();

        b.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        b.Property(x => x.PublishedAt).IsRequired();
        b.Property(x => x.IsPublished).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);
    }
}
