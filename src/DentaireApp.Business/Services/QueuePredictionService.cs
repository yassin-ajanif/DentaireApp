using DentaireApp.Business.Common;
using DentaireApp.Business.Contracts.Services;
using Microsoft.Extensions.Options;

namespace DentaireApp.Business.Services;

public sealed class QueuePredictionService(IOptions<QueuePredictionOptions> options) : IQueuePredictionService
{
    private readonly QueuePredictionOptions _options = options.Value;

    public Task<IReadOnlyList<QueuePredictionItem>> GetPredictionsAsync(QueueSnapshot queueSnapshot, CancellationToken cancellationToken = default)
    {
        var ordered = queueSnapshot.Appointments.OrderBy(a => a.QueueNumber).ToList();
        if (queueSnapshot.DoctorCheckInTime is null)
        {
            return Task.FromResult<IReadOnlyList<QueuePredictionItem>>(ordered
                .Select(a => new QueuePredictionItem(a.Id, a.QueueNumber, $"Patient {a.QueueNumber}", null, true))
                .ToList());
        }

        var result = new List<QueuePredictionItem>(ordered.Count);
        for (var index = 0; index < ordered.Count; index++)
        {
            var start = queueSnapshot.DoctorCheckInTime.Value.AddMinutes(index * _options.AverageConsultationMinutesFallback);
            var end = start.AddMinutes(_options.IntervalPredictionMinutes);
            result.Add(new QueuePredictionItem(
                ordered[index].Id,
                ordered[index].QueueNumber,
                $"Patient {ordered[index].QueueNumber}",
                $"{start:HH:mm} - {end:HH:mm}",
                false));
        }

        return Task.FromResult<IReadOnlyList<QueuePredictionItem>>(result);
    }
}

