using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Contracts.Repositories;

public interface ITreatmentRepository
{
    Task<IReadOnlyList<TreatmentSheet>> GetSheetsByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<TreatmentSheet?> GetSheetByIdAsync(Guid sheetId, CancellationToken cancellationToken = default);
    Task AddSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default);
    Task SaveSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default);
}

