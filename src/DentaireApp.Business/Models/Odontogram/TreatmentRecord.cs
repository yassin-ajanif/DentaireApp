namespace DentaireApp.Business.Models.Odontogram;

public sealed class TreatmentRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public string ToothCode { get; set; } = string.Empty;
    public string Surface { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

