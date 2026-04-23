using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Services;

public sealed class PatientRegistrationService(IPatientRepository patientRepository) : IPatientRegistrationService
{
    public async Task<Patient> RegisterOrResolvePatientAsync(PatientRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var nom = request.Nom.Trim();
        var telephone = request.Telephone.Trim();

        var existing = await patientRepository.GetByNomAndTelephoneAsync(nom, telephone, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var patient = new Patient
        {
            Nom = nom,
            Age = request.Age,
            Adresse = request.Adresse.Trim(),
            Telephone = telephone,
        };

        await patientRepository.AddAsync(patient, cancellationToken);
        return patient;
    }
}

