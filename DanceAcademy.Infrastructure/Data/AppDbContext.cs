#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.Role).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Email).IsUnique();
        });
    }
}