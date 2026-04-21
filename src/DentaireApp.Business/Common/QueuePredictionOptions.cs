namespace DentaireApp.Business.Common;

public sealed class QueuePredictionOptions
{
    public const string SectionName = "QueuePrediction";
    public int IntervalPredictionMinutes { get; set; } = 30;
    public int AverageConsultationMinutesFallback { get; set; } = 20;
}

