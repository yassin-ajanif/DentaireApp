namespace DentaireApp.Business.Models.Patients;

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nom { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string Telephone { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public List<TreatmentInfo> TreatmentInfos { get; set; } = [];
}

