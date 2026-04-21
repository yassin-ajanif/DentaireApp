namespace DentaireApp.Business.Models.Patients;

public sealed class TreatmentSheet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TreatmentLine> Lines { get; set; } = [];
}

public sealed class TreatmentLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TreatmentSheetId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string NatureOperation { get; set; } = string.Empty;
    public decimal PrixConven { get; set; }
    public decimal Recu { get; set; }
    public decimal ARecevoir { get; set; }
}

