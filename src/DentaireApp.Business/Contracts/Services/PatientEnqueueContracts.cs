using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;

namespace DentaireApp.Business.Contracts.Services;

public sealed record PatientEnqueueResult(Patient Patient, Appointment Appointment);

public interface IPatientEnqueueService
{
    Task<PatientEnqueueResult> RegisterAndEnqueueTodayAsync(
        PatientRegistrationRequest request,
        CancellationToken cancellationToken = default);
}
