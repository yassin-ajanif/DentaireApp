using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Patients;
using DentaireApp.Business.Validation;

namespace DentaireApp.Business.Services;

public sealed class PatientRecordService(
    IPatientRepository patientRepository,
    ITreatmentInfoRepository treatmentInfoRepository) : IPatientRecordService
{
    public Task<Patient?> GetPatientRecordAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        patientRepository.GetByIdAsync(patientId, cancellationToken);

    public Task<IReadOnlyList<TreatmentInfo>> GetTreatmentInfosAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        treatmentInfoRepository.GetByPatientIdAsync(patientId, cancellationToken);

    public async Task SaveTreatmentInfosAsync(Guid patientId, IReadOnlyList<TreatmentInfo> treatmentInfos, CancellationToken cancellationToken = default)
    {
        foreach (var treatmentInfo in treatmentInfos)
        {
            TreatmentInfoValidator.Validate(treatmentInfo);
        }

        await treatmentInfoRepository.ReplaceByPatientIdAsync(patientId, treatmentInfos, cancellationToken);
    }
}

