using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Services;

public sealed class PatientRegistrationService(IPatientRepository patientRepository) : IPatientRegistrationService
{
    public async Task<Patient> RegisterOrResolvePatientAsync(PatientRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await patientRepository.GetByNomAndTelephoneAsync(request.Nom, request.Telephone, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var patient = new Patient
        {
            Nom = request.Nom.Trim(),
            Age = request.Age,
            Adresse = request.Adresse.Trim(),
            Telephone = request.Telephone.Trim(),
        };

        await patientRepository.AddAsync(patient, cancellationToken);
        return patient;
    }
}

