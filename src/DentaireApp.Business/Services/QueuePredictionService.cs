using DentaireApp.Business.Common;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using Microsoft.Extensions.Options;

namespace DentaireApp.Business.Services;

public class QueuePredictionService(IOptions<QueuePredictionOptions> options) : IQueuePredictionService
{
    private readonly QueuePredictionOptions _options = options.Value;

    public Task<IReadOnlyList<QueuePredictionItem>> GetPredictionsAsync(QueueSnapshot queueSnapshot, CancellationToken cancellationToken = default)
    {
        var ordered = queueSnapshot.Appointments.OrderBy(a => a.QueueNumber).ToList();
        if (queueSnapshot.LastTerminatedAt is null)
        {
            return Task.FromResult<IReadOnlyList<QueuePredictionItem>>(ordered
                .Select(a => new QueuePredictionItem(a.Id, a.QueueNumber, $"Patient {a.QueueNumber}", null, true))
                .ToList());
        }

        var fallbackMinutes = _options.AverageConsultationMinutesFallback > 0
            ? _options.AverageConsultationMinutesFallback
            : QueuePredictionOptions.DefaultAverageConsultationMinutesFallback;
        var avgMinutes = queueSnapshot.AverageConsultationMinutesOverride is > 0
            ? queueSnapshot.AverageConsultationMinutesOverride.Value
            : fallbackMinutes;
        if (avgMinutes <= 0)
        {
            avgMinutes = fallbackMinutes;
        }

        // Forward slots from last terminé: only Waiting and InProgress consume slots; Done and Cancelled do not.
        var result = new List<QueuePredictionItem>(ordered.Count);
        var slotIndex = 0;
        foreach (var appointment in ordered)
        {
            if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Done)
            {
                result.Add(new QueuePredictionItem(
                    appointment.Id,
                    appointment.QueueNumber,
                    $"Patient {appointment.QueueNumber}",
                    null,
                    false));
                continue;
            }

            if (appointment.Status is AppointmentStatus.Waiting or AppointmentStatus.InProgress)
            {
                var start = queueSnapshot.LastTerminatedAt.Value.AddMinutes(slotIndex * avgMinutes);
                var end = start.AddMinutes(avgMinutes);
                slotIndex++;
                result.Add(new QueuePredictionItem(
                    appointment.Id,
                    appointment.QueueNumber,
                    $"Patient {appointment.QueueNumber}",
                    $"{start:HH:mm} - {end:HH:mm}",
                    false));
                continue;
            }

            result.Add(new QueuePredictionItem(
                appointment.Id,
                appointment.QueueNumber,
                $"Patient {appointment.QueueNumber}",
                null,
                false));
        }

        return Task.FromResult<IReadOnlyList<QueuePredictionItem>>(result);
    }
}
