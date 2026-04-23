using DentaireApp.Business.Models.Appointments;

namespace DentaireApp.Business.Interfaces.Services;

public sealed record QueueQuery(DateOnly Date, int Page = 1, int PageSize = 20, string? SearchTerm = null);

public sealed record QueuePredictionItem(Guid AppointmentId, int QueueNumber, string PatientNom, string? IntervalLabel, bool IsPendingDoctorArrival);

public sealed record QueueSnapshot(IReadOnlyList<Appointment> Appointments, DateTime? DoctorCheckInTime);

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
