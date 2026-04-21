using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Contracts.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Patient?> GetByNomAndTelephoneAsync(string nom, string telephone, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
}

