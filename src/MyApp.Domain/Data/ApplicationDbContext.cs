using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Model;

namespace MyApp.Domain.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<User> ApplicationUsers => Set<User>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Instruction> Instructions => Set<Instruction>();
    public DbSet<Medicine> Medicines => Set<Medicine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(p => p.Pharmacist)
            .HasForeignKey(p => p.PharmacistId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Review>()
            .Property(r => r.DateCreated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Patient) 
            .WithMany()
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Entry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
            e.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Review)
            .WithMany(r => r.Entries)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Instruction>(e =>
        {
            e.ToTable("Instructions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Text).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Medicine>(e =>
        {
            e.ToTable("Medicines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });
    }
}