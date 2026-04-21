using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Contracts.Services;

public sealed record PatientRegistrationRequest(string Nom, int Age, string Adresse, string Telephone);

public interface IPatientRegistrationService
{
    Task<Patient> RegisterOrResolvePatientAsync(PatientRegistrationRequest request, CancellationToken cancellationToken = default);
}

public interface IPatientRecordService
{
    Task<Patient?> GetPatientRecordAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreatmentSheet>> GetTreatmentSheetsAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<TreatmentSheet> CreateTreatmentSheetAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task SaveTreatmentSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default);
}

