using DentaireApp.Business.Models.Appointments;

namespace DentaireApp.Business.Interfaces.Repositories;

public interface IAppointmentRepository
{
    Task<IReadOnlyList<Appointment>> GetQueueAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<int> GetNextQueueNumberAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
