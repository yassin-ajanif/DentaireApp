using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Services;

public class AppointmentService(
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    IAppointmentEnqueuePersistence enqueuePersistence) : IAppointmentService
{
    public Task<Patient?> FindExistingPatientByTelephoneAsync(
        string telephone,
        CancellationToken cancellationToken = default) =>
        patientRepository.GetByTelephoneAsync(telephone.Trim(), cancellationToken);

    public async Task<AppointmentEnqueueResult> EnqueueAsync(
        Patient patientDraft,
        Appointment appointmentDraft,
        CancellationToken cancellationToken = default)
    {
        var normalizedPatient = new Patient
        {
            Nom = patientDraft.Nom.Trim(),
            Age = patientDraft.Age,
            Adresse = patientDraft.Adresse.Trim(),
            Telephone = patientDraft.Telephone.Trim(),
        };

        var normalizedAppointment = new Appointment
        {
            Status = appointmentDraft.Status,
            StartedAt = appointmentDraft.StartedAt,
            CompletedAt = appointmentDraft.CompletedAt,
        };

        return await enqueuePersistence.EnqueueAsync(
            normalizedPatient, normalizedAppointment, cancellationToken);
    }

    public async Task SetInProgressAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return;
        }

        appointment.Status = AppointmentStatus.InProgress;
        appointment.StartedAt ??= DateTime.Now;
        appointment.CompletedAt = null;
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    public async Task<bool> SetDoneAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return false;
        }

        appointment.Status = AppointmentStatus.Done;
        appointment.StartedAt ??= DateTime.Now;
        appointment.CompletedAt = DateTime.Now;
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        return true;
    }

    public async Task SetWaitingAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return;
        }

        appointment.Status = AppointmentStatus.Waiting;
        appointment.CompletedAt = null;
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    public async Task SetCancelledAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return;
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
