using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using DentaireApp.UI.Avalonia.ViewModels;

namespace DentaireApp.UI.Tests;

public class UnitTest1
{
    [Fact]
    public async Task QueueViewModel_NavigatesToNextItem()
    {
        var patientId = Guid.NewGuid();
        var patient = new Patient
        {
            Id = patientId,
            Nom = "Test",
            Age = 30,
            Adresse = "",
            Telephone = "1",
        };

        var queueService = new FakeQueueService(
        [
            new Appointment { PatientId = patientId, QueueNumber = 1, Status = AppointmentStatus.Waiting },
            new Appointment { PatientId = patientId, QueueNumber = 2, Status = AppointmentStatus.Waiting },
        ]);

        var vm = new QueueViewModel(queueService, new FakePatientRepository(patient), new FakePatientEnqueueService());
        await vm.InitializeAsync();

        vm.SelectedItem = vm.Items[0];
        vm.SuivantCommand.Execute(null);
        Assert.NotNull(vm.SelectedItem);
        Assert.Equal(2, vm.SelectedItem.Number);
    }

    private sealed class FakePatientRepository(Patient patient) : IPatientRepository
    {
        public Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Patient?>(patientId == patient.Id ? patient : null);

        public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Patient>>([patient]);

        public Task<Patient?> GetByNomAndTelephoneAsync(string nom, string telephone, CancellationToken cancellationToken = default) =>
            Task.FromResult<Patient?>(null);

        public Task AddAsync(Patient p, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(Patient p, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeQueueService(IReadOnlyList<Appointment> appointments) : IQueueService
    {
        public Task<IReadOnlyList<Appointment>> GetQueueAsync(QueueQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult(appointments);

        public Task<Appointment> CreateTicketForPatientAsync(Guid patientId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SetActiveTicketAsync(Guid appointmentId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task MoveNextAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task MovePreviousAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePatientEnqueueService : IPatientEnqueueService
    {
        public Task<PatientEnqueueResult> RegisterAndEnqueueTodayAsync(
            PatientRegistrationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
