using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Services;

public class PatientRecordService(
    IPatientRepository patientRepository,
    ITreatmentInfoRepository treatmentInfoRepository) : IPatientRecordService
{
    public Task<Patient?> GetPatientRecordAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        patientRepository.GetByIdAsync(patientId, cancellationToken);

    public Task<IReadOnlyList<TreatmentInfo>> GetTreatmentInfosAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        treatmentInfoRepository.GetByPatientIdAsync(patientId, cancellationToken);

    public async Task SaveTreatmentInfosAsync(Guid patientId, IReadOnlyList<TreatmentInfo> treatmentInfos, CancellationToken cancellationToken = default)
    {
        await treatmentInfoRepository.ReplaceByPatientIdAsync(patientId, treatmentInfos, cancellationToken);
    }
}

