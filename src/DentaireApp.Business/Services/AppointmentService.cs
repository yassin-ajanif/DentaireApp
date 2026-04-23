using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Services;

public class AppointmentService(
    IPatientRepository patientRepository,
    IAppointmentEnqueuePersistence enqueuePersistence) : IAppointmentService
{
    public Task<Patient?> FindExistingPatientByTelephoneAsync(
        string telephone,
        CancellationToken cancellationToken = default) =>
        patientRepository.GetByTelephoneAsync(telephone.Trim(), cancellationToken);

    public async Task<AppointmentEnqueueResult> EnqueueAsync(
        Patient patientDraft,
        Appointment appointmentDraft,
        CancellationToken cancellationToken = default)
    {
        var normalizedPatient = new Patient
        {
            Nom = patientDraft.Nom.Trim(),
            Age = patientDraft.Age,
            Adresse = patientDraft.Adresse.Trim(),
            Telephone = patientDraft.Telephone.Trim(),
        };

        var normalizedAppointment = new Appointment
        {
            Status = appointmentDraft.Status,
        };

        return await enqueuePersistence.EnqueueAsync(
            normalizedPatient, normalizedAppointment, cancellationToken);
    }
}
