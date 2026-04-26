using System.Collections.Generic;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Interfaces.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>Loads patients matching <paramref name="patientIds"/> (duplicates ignored). Empty input returns an empty list.</summary>
    Task<IReadOnlyList<Patient>> GetByIdsAsync(
        IReadOnlyCollection<Guid> patientIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Paged patients ordered by <see cref="Patient.Nom"/>. Optional <paramref name="searchTerm"/> filters by name or phone (case-insensitive).
    /// </summary>
    Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPatientsPageAsync(
        int skip,
        int take,
        string? searchTerm,
        CancellationToken cancellationToken = default);
    Task<Patient?> GetByTelephoneAsync(string telephone, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);

    /// <summary>Deletes the patient and related appointments and treatment rows in one transaction.</summary>
    /// <returns>False if the patient does not exist.</returns>
    Task<bool> DeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default);
}
