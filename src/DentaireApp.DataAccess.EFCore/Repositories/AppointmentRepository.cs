using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.DataAccess.EFCore;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class AppointmentRepository(AppDbContext dbContext) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
        dbContext.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetQueueAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var (utcStart, utcEnd) = QueueDayBounds.ForLocalCalendarDate(date);
        return await dbContext.Appointments
            .Where(x => x.StartedAt != null && x.StartedAt >= utcStart && x.StartedAt <= utcEnd)
            .OrderBy(x => x.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextQueueNumberAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var (utcStart, utcEnd) = QueueDayBounds.ForLocalCalendarDate(date);
        var max = await dbContext.Appointments
            .Where(x => x.StartedAt != null && x.StartedAt >= utcStart && x.StartedAt <= utcEnd)
            .Select(x => (int?)x.QueueNumber)
            .MaxAsync(cancellationToken) ?? 0;
        return max + 1;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Update(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

