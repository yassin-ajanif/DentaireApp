using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Interfaces.Services;

public interface IPatientRecordService
{
    Task<Patient?> GetPatientRecordAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreatmentInfo>> GetTreatmentInfosAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task SaveTreatmentInfosAsync(Guid patientId, IReadOnlyList<TreatmentInfo> treatmentInfos, CancellationToken cancellationToken = default);
}
