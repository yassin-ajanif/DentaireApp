using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Billing;
using DentaireApp.Business.Models.Odontogram;
using DentaireApp.Business.Models.Patients;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TreatmentSheet> TreatmentSheets => Set<TreatmentSheet>();
    public DbSet<TreatmentLine> TreatmentLines => Set<TreatmentLine>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<TreatmentRecord> TreatmentRecords => Set<TreatmentRecord>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nom).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Telephone).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => new { x.Nom, x.Telephone }).IsUnique();
            entity.HasMany(x => x.TreatmentSheets).WithOne().HasForeignKey(x => x.PatientId);
        });

        modelBuilder.Entity<TreatmentSheet>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.TreatmentSheetId);
        });

        modelBuilder.Entity<TreatmentLine>().HasKey(x => x.Id);
        modelBuilder.Entity<Appointment>().HasKey(x => x.Id);
        modelBuilder.Entity<TreatmentRecord>().HasKey(x => x.Id);
        modelBuilder.Entity<Invoice>().HasKey(x => x.Id);
        modelBuilder.Entity<InvoiceLine>().HasKey(x => x.Id);
        modelBuilder.Entity<Payment>().HasKey(x => x.Id);
    }
}

