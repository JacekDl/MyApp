using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain;

namespace MyApp.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> ApplicationUsers => Set<User>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(p => p.CreatedByUser)
            .HasForeignKey(p => p.CreatedByUserId);

        modelBuilder.Entity<Review>(r =>
        {
            r.HasKey(r => r.Id);
            r.Property(e => e.Advice).IsRequired();
        });
            

        modelBuilder.Entity<Review>()
            .Property(r => r.DateCreated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        //modelBuilder.Entity<Review>()
        //    .HasOne(r => r.CreatedByUser)
        //    .WithMany()
        //    .HasForeignKey(r => r.CreatedByUserId)
        //    .OnDelete(DeleteBehavior.Restrict);
    }
}