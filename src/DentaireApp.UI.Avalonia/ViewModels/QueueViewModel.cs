using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public partial class QueueViewModel : ViewModelBase
{
    [ObservableProperty]
    private string searchTerm = string.Empty;

    [ObservableProperty]
    private QueueItemViewModel? selectedItem;
    public event Func<QueueItemViewModel?, Task>? SelectedItemChanged;

    public Func<Task<NewPatientInput?>>? RequestNewPatientAsync { get; set; }

    /// <summary>Existing patient name; return true to enqueue anyway.</summary>
    public Func<string, Task<bool>>? ConfirmDuplicatePatientEnqueueAsync { get; set; }
    private readonly IQueueService queueService;
    private readonly IPatientRepository patientRepository;
    private readonly IAppointmentService appointmentService;
    public ObservableCollection<QueueItemViewModel> Items { get; } = [];
    private readonly ObservableCollection<QueueItemViewModel> allQueueItems = [];
    private readonly ObservableCollection<QueueItemViewModel> allPatients = [];
    public ReadOnlyObservableCollection<QueueItemViewModel> AllPatients { get; }

    public QueueViewModel(
        IQueueService queueService,
        IPatientRepository patientRepository,
        IAppointmentService appointmentService)
    {
        this.queueService = queueService;
        this.patientRepository = patientRepository;
        this.appointmentService = appointmentService;
        AllPatients = new ReadOnlyObservableCollection<QueueItemViewModel>(allPatients);
    }

    partial void OnSearchTermChanged(string value) => ApplySearchFilter();
    partial void OnSelectedItemChanged(QueueItemViewModel? value)
    {
        if (SelectedItemChanged is not null)
        {
            _ = SelectedItemChanged.Invoke(value);
        }
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task NouveauNumero()
    {
        if (RequestNewPatientAsync is null)
        {
            return;
        }

        var input = await RequestNewPatientAsync();
        if (input is null)
        {
            return;
        }

        try
        {
            var telephone = input.Telephone.Trim();
            var existing = await appointmentService.FindExistingPatientByTelephoneAsync(telephone);
            if (existing is not null)
            {
                if (ConfirmDuplicatePatientEnqueueAsync is null ||
                    !await ConfirmDuplicatePatientEnqueueAsync(existing.Nom))
                {
                    return;
                }
            }

            var patient = new Patient
            {
                Nom = input.Nom,
                Age = input.Age,
                Adresse = input.Adresse,
                Telephone = input.Telephone,
            };
            var appointment = new Appointment { Status = AppointmentStatus.Waiting };
            var result = await appointmentService.EnqueueAsync(patient, appointment);

            await ReloadAsync();
            SelectedItem = Items.FirstOrDefault(x =>
                x.PatientId == result.Patient.Id && x.Number == result.Appointment.QueueNumber);
        }
        catch
        {
            // Prevent UI crash from persistence errors (e.g., unique-key races); keep window alive.
        }
    }

    public async Task UpdatePatientCredentialsAsync(QueueItemViewModel item, NewPatientInput input)
    {
        var patient = await patientRepository.GetByIdAsync(item.PatientId);
        if (patient is null)
        {
            return;
        }

        patient.Nom = input.Nom.Trim();
        patient.Age = input.Age;
        patient.Adresse = input.Adresse.Trim();
        patient.Telephone = input.Telephone.Trim();
        await patientRepository.UpdateAsync(patient);

        await ReloadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.PatientId == patient.Id && x.Number == item.Number);
    }

    [RelayCommand]
    private void Suivant()
    {
        if (Items.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedItem is null ? -1 : Items.IndexOf(SelectedItem);
        var nextIndex = Math.Min(currentIndex + 1, Items.Count - 1);
        SelectedItem = Items[nextIndex];
    }

    [RelayCommand]
    private void Precedent()
    {
        if (Items.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedItem is null ? 0 : Items.IndexOf(SelectedItem);
        var previousIndex = Math.Max(currentIndex - 1, 0);
        SelectedItem = Items[previousIndex];
    }

    private async Task ReloadAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        var patientsById = patients.ToDictionary(p => p.Id);

        var queue = await queueService.GetQueueAsync(new QueueQuery(DateOnly.FromDateTime(DateTime.Now), 1, 500, null));

        allQueueItems.Clear();
        foreach (var appointment in queue)
        {
            if (!patientsById.TryGetValue(appointment.PatientId, out var patient))
            {
                continue;
            }

            allQueueItems.Add(new QueueItemViewModel(
                appointment.PatientId,
                appointment.QueueNumber,
                patient.Nom,
                "15:30 - 16:15",
                patient.Age,
                patient.Adresse,
                patient.Telephone));
        }

        allPatients.Clear();
        var index = 1;
        foreach (var patient in patients)
        {
            allPatients.Add(new QueueItemViewModel(
                patient.Id,
                index++,
                patient.Nom,
                string.Empty,
                patient.Age,
                patient.Adresse,
                patient.Telephone));
        }

        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        var term = SearchTerm?.Trim();
        IEnumerable<QueueItemViewModel> filtered = string.IsNullOrWhiteSpace(term)
            ? allQueueItems
            : allQueueItems.Where(x => x.Nom.Contains(term, StringComparison.OrdinalIgnoreCase));

        Items.Clear();
        foreach (var item in filtered)
        {
            Items.Add(item);
        }

        if (SelectedItem is not null && !Items.Contains(SelectedItem))
        {
            SelectedItem = Items.FirstOrDefault();
        }
    }
}

public sealed partial class QueueItemViewModel : ObservableObject
{
    public Guid PatientId { get; }
    public int Number { get; }

    [ObservableProperty]
    private string nom;

    [ObservableProperty]
    private string predictedInterval;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string adresse;

    [ObservableProperty]
    private string telephone;

    public QueueItemViewModel(Guid patientId, int number, string nom, string predictedInterval, int age, string adresse, string telephone)
    {
        PatientId = patientId;
        Number = number;
        this.nom = nom;
        this.predictedInterval = predictedInterval;
        this.age = age;
        this.adresse = adresse;
        this.telephone = telephone;
    }
}

public sealed record NewPatientInput(string Nom, int Age, string Adresse, string Telephone);

