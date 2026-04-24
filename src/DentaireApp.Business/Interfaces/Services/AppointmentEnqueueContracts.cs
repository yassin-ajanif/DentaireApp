using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Interfaces.Services;

public sealed record AppointmentEnqueueResult(Patient Patient, Appointment Appointment);

public interface IAppointmentService
{
    Task<Patient?> FindExistingPatientByTelephoneAsync(
        string telephone,
        CancellationToken cancellationToken = default);

    Task<AppointmentEnqueueResult> EnqueueAsync(
        Patient patientDraft,
        Appointment appointmentDraft,
        CancellationToken cancellationToken = default);

    Task SetInProgressAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    /// <returns>False if the appointment was not found.</returns>
    Task<bool> SetDoneAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task SetWaitingAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task SetCancelledAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// DataAccess port: one EF transaction for resolve/insert patient + insert appointment.
/// </summary>
public interface IAppointmentEnqueuePersistence
{
    Task<AppointmentEnqueueResult> EnqueueAsync(
        Patient patient,
        Appointment appointment,
        CancellationToken cancellationToken = default);
}
