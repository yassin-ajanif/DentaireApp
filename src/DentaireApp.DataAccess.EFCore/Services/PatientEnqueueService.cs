using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Services;

public class PatientEnqueueService(AppDbContext db) : IAppointmentEnqueuePersistence
{
    public async Task<AppointmentEnqueueResult> EnqueueAsync(
        Patient patientDraft,
        Appointment appointmentDraft,
        CancellationToken cancellationToken = default)
    {
        var telephone = patientDraft.Telephone;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var patient = await db.Patients
                .FirstOrDefaultAsync(x => x.Telephone == telephone, cancellationToken);

            if (patient is null)
            {
                patient = new Patient
                {
                    Nom = patientDraft.Nom,
                    Age = patientDraft.Age,
                    Adresse = patientDraft.Adresse,
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
                Status = appointmentDraft.Status,
            };
            db.Appointments.Add(appointment);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AppointmentEnqueueResult(patient, appointment);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
