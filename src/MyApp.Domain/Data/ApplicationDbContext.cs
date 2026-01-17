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
    public DbSet<Instruction> Instructions => Set<Instruction>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<PharmacistPromotionRequest> PharmacistPromotionRequests => Set<PharmacistPromotionRequest>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentPlanMedicine> TreatmentPlanMedicines => Set<TreatmentPlanMedicine>();
    public DbSet<TreatmentPlanAdvice> TreatmentPlanAdvices => Set<TreatmentPlanAdvice>();
    public DbSet<MedicineTakenConfirmation> MedicineTakenConfirmations => Set<MedicineTakenConfirmation>();
    public DbSet<TreatmentPlanReview> TreatmentPlanReviews => Set<TreatmentPlanReview>();
    public DbSet<ReviewEntry> ReviewEntries => Set<ReviewEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Instruction>(e =>
        {
            e.ToTable("Instructions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(Instruction.CodeMaxLength).IsRequired();
            e.Property(x => x.Text).HasMaxLength(Instruction.TextMaxLength).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Medicine>(e =>
        {
            e.ToTable("Medicines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(Medicine.CodeMaxLength).IsRequired();
            e.Property(x => x.Name).HasMaxLength(Medicine.NameMaxLength).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<PharmacistPromotionRequest>(e =>
        {
            e.Property(x => x.NumerPWZF)
                .HasMaxLength(PharmacistPromotionRequest.NumerPWZFLength)
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

            e.HasOne(x => x.Review)
                .WithOne(r => r.TreatmentPlan)
                .HasForeignKey<TreatmentPlanReview>(r => r.IdTreatmentPlan)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<TreatmentPlanMedicine>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.MedicineName)
                .IsRequired()
                .HasMaxLength(TreatmentPlanMedicine.MedicineNameMaxLength);

            e.Property(x => x.Dosage)
                .IsRequired()
                .HasMaxLength(TreatmentPlanMedicine.MedicineNameMaxLength);

            e.Property(x => x.TimeOfDay)
                .IsRequired();
        });

        modelBuilder.Entity<TreatmentPlanAdvice>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.AdviceText)
                .HasMaxLength(TreatmentPlanAdvice.AdviceTextMaxLength);
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

        modelBuilder.Entity<TreatmentPlanReview>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.IdTreatmentPlan)
                .IsUnique();
            e.Property(x => x.UnreadForPatient)
                .HasDefaultValue(false);
            e.Property(x => x.UnreadForPharmacist)
                .HasDefaultValue(false);

            e.HasMany(x => x.ReviewEntries)
                .WithOne(x => x.TreatmentPlanReview)
                .HasForeignKey(x => x.IdTreatmentPlanReview)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewEntry>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(500);

            e.Property(x => x.Author)
                .HasConversion<int>()
                .IsRequired();

            e.Property(x => x.DateCreated)
                .IsRequired();

            e.HasIndex(x => new { x.IdTreatmentPlanReview, x.DateCreated });
        });

    }
}