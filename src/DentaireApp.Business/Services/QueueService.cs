using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Appointments;

namespace DentaireApp.Business.Services;

public sealed class QueueService(IAppointmentRepository appointmentRepository) : IQueueService
{
    public async Task<IReadOnlyList<Appointment>> GetQueueAsync(QueueQuery query, CancellationToken cancellationToken = default)
    {
        var queue = await appointmentRepository.GetQueueAsync(query.Date, cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? queue
            : queue.Where(a => a.QueueNumber.ToString().Contains(query.SearchTerm.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        return filtered
            .OrderBy(a => a.QueueNumber)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
    }

    public async Task<Appointment> CreateTicketForPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var nextNumber = await appointmentRepository.GetNextQueueNumberAsync(today, cancellationToken);
        var appointment = new Appointment
        {
            PatientId = patientId,
            QueueNumber = nextNumber,
            Status = AppointmentStatus.Waiting,
        };

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        return appointment;
    }

    public Task SetActiveTicketAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task MoveNextAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task MovePreviousAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

