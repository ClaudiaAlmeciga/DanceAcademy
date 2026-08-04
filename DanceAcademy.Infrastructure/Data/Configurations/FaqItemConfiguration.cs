#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> b)
    {
        b.ToTable("FaqItems");

        b.HasKey(x => x.Id);

        b.Property(x => x.Question)
            .HasMaxLength(300)
            .IsRequired();

        b.Property(x => x.Answer)
            .HasMaxLength(4000)
            .IsRequired();

        b.Property(x => x.Category)
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Order).IsRequired();
        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);
    }
}
