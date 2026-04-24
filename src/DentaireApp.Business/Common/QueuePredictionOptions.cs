namespace DentaireApp.Business.Common;

public sealed class QueuePredictionOptions
{
    public const string SectionName = "QueuePrediction";

    public const int DefaultAverageConsultationMinutesFallback = 20;

    /// <summary>
    /// Optional configuration (section <see cref="SectionName"/>): minutes per slot when the queue snapshot has no positive average from UI settings.
    /// If unset or zero, <see cref="DefaultAverageConsultationMinutesFallback"/> is used.
    /// </summary>
    public int AverageConsultationMinutesFallback { get; set; } = DefaultAverageConsultationMinutesFallback;
}

