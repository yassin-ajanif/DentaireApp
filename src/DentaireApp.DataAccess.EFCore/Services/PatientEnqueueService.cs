using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Services;

public sealed class PatientEnqueueService(AppDbContext db) : IPatientEnqueueService
{
    public async Task<PatientEnqueueResult> RegisterAndEnqueueTodayAsync(
        PatientRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var nom = request.Nom.Trim();
        var telephone = request.Telephone.Trim();
        var adresse = request.Adresse.Trim();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var patient = await db.Patients
                .FirstOrDefaultAsync(
                    x => x.Nom == nom && x.Telephone == telephone,
                    cancellationToken);

            if (patient is null)
            {
                patient = new Patient
                {
                    Nom = nom,
                    Age = request.Age,
                    Adresse = adresse,
                    Telephone = telephone,
                };
                db.Patients.Add(patient);
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var dayStart = today.ToDateTime(TimeOnly.MinValue);
            var dayEnd = today.ToDateTime(TimeOnly.MaxValue);

            var maxQueue = await db.Appointments
                .Where(x => x.StartedAt == null
                    || (x.StartedAt >= dayStart && x.StartedAt <= dayEnd))
                .Select(x => (int?)x.QueueNumber)
                .MaxAsync(cancellationToken) ?? 0;

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                QueueNumber = maxQueue + 1,
                Status = AppointmentStatus.Waiting,
            };
            db.Appointments.Add(appointment);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PatientEnqueueResult(patient, appointment);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
