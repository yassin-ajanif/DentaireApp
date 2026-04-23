using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Contracts.Repositories;

public interface ITreatmentInfoRepository
{
    Task<IReadOnlyList<TreatmentInfo>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<TreatmentInfo?> GetByIdAsync(Guid treatmentInfoId, CancellationToken cancellationToken = default);
    Task AddAsync(TreatmentInfo treatmentInfo, CancellationToken cancellationToken = default);
    Task SaveAsync(TreatmentInfo treatmentInfo, CancellationToken cancellationToken = default);
    Task ReplaceByPatientIdAsync(Guid patientId, IReadOnlyList<TreatmentInfo> treatmentInfos, CancellationToken cancellationToken = default);
}

