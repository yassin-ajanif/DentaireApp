using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Patients;
using DentaireApp.Business.Validation;

namespace DentaireApp.Business.Services;

public sealed class PatientRecordService(
    IPatientRepository patientRepository,
    ITreatmentRepository treatmentRepository) : IPatientRecordService
{
    public Task<Patient?> GetPatientRecordAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        patientRepository.GetByIdAsync(patientId, cancellationToken);

    public Task<IReadOnlyList<TreatmentSheet>> GetTreatmentSheetsAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        treatmentRepository.GetSheetsByPatientIdAsync(patientId, cancellationToken);

    public async Task<TreatmentSheet> CreateTreatmentSheetAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var sheet = new TreatmentSheet
        {
            PatientId = patientId,
        };

        await treatmentRepository.AddSheetAsync(sheet, cancellationToken);
        return sheet;
    }

    public async Task SaveTreatmentSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default)
    {
        TreatmentSheetValidator.Validate(sheet);
        await treatmentRepository.SaveSheetAsync(sheet, cancellationToken);
    }
}

