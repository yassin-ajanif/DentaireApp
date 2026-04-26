using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Interfaces.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Patient?> GetByTelephoneAsync(string telephone, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);

    /// <summary>Deletes the patient and related appointments and treatment rows in one transaction.</summary>
    /// <returns>False if the patient does not exist.</returns>
    Task<bool> DeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default);
}
