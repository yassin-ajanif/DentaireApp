using DentaireApp.Business.Models.Appointments;

namespace DentaireApp.Business.Interfaces.Services;

public sealed record QueueQuery(DateOnly Date, int Page = 1, int PageSize = 20, string? SearchTerm = null);

public sealed record QueuePredictionItem(Guid AppointmentId, int QueueNumber, string PatientNom, string? IntervalLabel, bool IsPendingDoctorArrival);

/// <param name="AverageConsultationMinutesOverride">When set (positive), overrides appsettings average minutes per slot (e.g. from UI settings).</param>
/// <param name="LastTerminatedAt">Instant of the last consultation marked done; predictions extend from here. Null until the first terminé of the day.</param>
public sealed record QueueSnapshot(
    IReadOnlyList<Appointment> Appointments,
    DateTime? LastTerminatedAt,
    int? AverageConsultationMinutesOverride = null);

public interface IQueueService
{
    Task<IReadOnlyList<Appointment>> GetQueueAsync(QueueQuery query, CancellationToken cancellationToken = default);
    Task<Appointment> CreateTicketForPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task SetActiveTicketAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task MoveNextAsync(CancellationToken cancellationToken = default);
    Task MovePreviousAsync(CancellationToken cancellationToken = default);
}

public interface IQueuePredictionService
{
    Task<IReadOnlyList<QueuePredictionItem>> GetPredictionsAsync(QueueSnapshot queueSnapshot, CancellationToken cancellationToken = default);
}
