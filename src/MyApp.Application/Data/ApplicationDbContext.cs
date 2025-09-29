using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain;

namespace MyApp.Application.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> ApplicationUsers => Set<User>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Entry> Entries => Set<Entry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(p => p.Pharmacist)
            .HasForeignKey(p => p.PharmacistId);

        modelBuilder.Entity<Review>()
            .Property(r => r.DateCreated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
            e.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Review)
            .WithMany(r => r.Entries)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}