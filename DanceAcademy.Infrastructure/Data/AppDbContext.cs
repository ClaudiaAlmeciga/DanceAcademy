#nullable enable
using DanceAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DanceAcademy.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Level> Levels => Set<Level>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}