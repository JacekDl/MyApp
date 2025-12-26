using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Model;
using MyApp.Model.enums;

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
    public DbSet<PharmacistPromotionRequest> PharmacistPromotionRequests => Set<PharmacistPromotionRequest>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanMedicine> TreatmentPlanMedicines => Set<TreatmentPlanMedicine>();
    public DbSet<TreatmentPlanAdvice> TreatmentPlanAdvices => Set<TreatmentPlanAdvice>();
    public DbSet<MedicineTakenConfirmation> MedicineTakenConfirmations => Set<MedicineTakenConfirmation>();

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

        modelBuilder.Entity<PharmacistPromotionRequest>(e =>
        {
            e.Property(x => x.NumerPWZF)
                .HasMaxLength(8)
                .IsRequired();
        });

        modelBuilder.Entity<TreatmentPlan>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.DateCreated).IsRequired();
            e.Property(x => x.Number).IsRequired();

            e.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(TreatmentPlanStatus.Created)
                .IsRequired();

            e.HasOne(x => x.Pharmacist)
                .WithMany()
                .HasForeignKey(x => x.IdPharmacist)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.IdPatient)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.Medicines)
                .WithOne(m => m.TreatmentPlan)
                .HasForeignKey(m => m.IdTreatmentPlan)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Advice)
                .WithOne(a => a.TreatmentPlan)
                .HasForeignKey<TreatmentPlanAdvice>(a => a.IdTreatmentPlan)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<TreatmentPlanMedicine>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.MedicineName)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.Dosage)
                .IsRequired()
                .HasMaxLength(100);

            e.Property(x => x.TimeOfDay)
                .IsRequired();
        });

        modelBuilder.Entity<TreatmentPlanAdvice>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.AdviceText)
                .HasMaxLength(2000);
        });

        modelBuilder.Entity<MedicineTakenConfirmation>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.DateTimeTaken)
                .IsRequired();

            e.HasOne(x => x.TreatmentPlanMedicine)
                .WithMany()
                .HasForeignKey(x => x.IdTreatmentPlanMedicine)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}