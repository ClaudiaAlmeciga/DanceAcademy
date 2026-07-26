#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanceAcademy.Infrastructure.Data.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> b)
    {
        b.ToTable("SubscriptionPlans");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(2000);

        b.Property(x => x.Price)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        b.Property(x => x.BillingPeriodDays).IsRequired();
        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        b.HasIndex(x => x.Name).IsUnique();
    }
}
