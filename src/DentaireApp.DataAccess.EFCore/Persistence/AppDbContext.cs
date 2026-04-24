using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<TreatmentInfo> TreatmentInfos => Set<TreatmentInfo>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nom).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Telephone).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.Telephone).IsUnique();
            entity.HasMany(x => x.TreatmentInfos)
                .WithOne()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TreatmentInfo>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NatureOperation).HasMaxLength(300).IsRequired();
            entity.HasIndex(x => new { x.PatientId, x.Date });
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.StartedAt)
                .HasConversion(
                    value => value.HasValue
                        ? new DateTimeOffset(value.Value.Kind == DateTimeKind.Utc
                            ? value.Value
                            : value.Value.ToUniversalTime()).ToUnixTimeSeconds()
                        : (long?)null,
                    value => value.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(value.Value).UtcDateTime
                        : (DateTime?)null)
                .HasColumnType("INTEGER");
            entity.Property(x => x.CompletedAt)
                .HasConversion(
                    value => value.HasValue
                        ? new DateTimeOffset(value.Value.Kind == DateTimeKind.Utc
                            ? value.Value
                            : value.Value.ToUniversalTime()).ToUnixTimeSeconds()
                        : (long?)null,
                    value => value.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(value.Value).UtcDateTime
                        : (DateTime?)null)
                .HasColumnType("INTEGER");
        });
    }
}

