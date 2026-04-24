using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Appointments;
using DentaireApp.Business.Models.Patients;
using DentaireApp.UI.Avalonia.Services;
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
    private readonly IQueuePredictionService queuePredictionService;
    private readonly UiSettingsFileService uiSettingsService;
    public ObservableCollection<QueueItemViewModel> Items { get; } = [];
    private readonly ObservableCollection<QueueItemViewModel> allQueueItems = [];
    private readonly ObservableCollection<QueueItemViewModel> allPatients = [];
    public ReadOnlyObservableCollection<QueueItemViewModel> AllPatients { get; }

    public QueueViewModel(
        IQueueService queueService,
        IPatientRepository patientRepository,
        IAppointmentService appointmentService,
        IQueuePredictionService queuePredictionService,
        UiSettingsFileService uiSettingsService)
    {
        this.queueService = queueService;
        this.patientRepository = patientRepository;
        this.appointmentService = appointmentService;
        this.queuePredictionService = queuePredictionService;
        this.uiSettingsService = uiSettingsService;
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
            var appointment = new Appointment { Status = AppointmentStatus.Waiting, StartedAt = DateTime.Now };
            var result = await appointmentService.EnqueueAsync(patient, appointment);

            await ReloadAsync();
            SelectedItem = Items.FirstOrDefault(x =>
                x.PatientId == result.Patient.Id &&
                x.Number == result.Appointment.QueueNumber &&
                x.AppointmentId == result.Appointment.Id);
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

    private async Task ReloadAsync()
    {
        var patients = await patientRepository.GetAllAsync();
        var patientsById = patients.ToDictionary(p => p.Id);

        var queue = await queueService.GetQueueAsync(new QueueQuery(DateOnly.FromDateTime(DateTime.Now), 1, 500, null));

        var ordered = queue.ToList();
        // "Next" green line: first waiting by queue order. Do not use max(Done): a later ticket can be
        // terminé while smaller numbers are still en attente — they must stay ahead of the line.
        var inProgressAppointment = ordered
            .Where(a => a.Status == AppointmentStatus.InProgress)
            .OrderBy(a => a.QueueNumber)
            .FirstOrDefault();
        var nextAppointment = inProgressAppointment is not null
            ? ordered
                .Where(a => a.Status == AppointmentStatus.Waiting && a.QueueNumber > inProgressAppointment.QueueNumber)
                .OrderBy(a => a.QueueNumber)
                .FirstOrDefault()
            : ordered
                .Where(a => a.Status == AppointmentStatus.Waiting)
                .OrderBy(a => a.QueueNumber)
                .FirstOrDefault();

        var settings = await uiSettingsService.LoadAsync();
        var avgOverride = settings.AverageTimeSpentWithPatient > 0 ? settings.AverageTimeSpentWithPatient : (int?)null;
        var snapshot = new QueueSnapshot(ordered, settings.LastTerminatedAt, avgOverride);
        var predictions = await queuePredictionService.GetPredictionsAsync(snapshot);
        var predictionByAppointmentId = predictions.ToDictionary(p => p.AppointmentId);

        allQueueItems.Clear();
        foreach (var appointment in ordered)
        {
            if (!patientsById.TryGetValue(appointment.PatientId, out var patient))
            {
                continue;
            }

            var (statusLabel, statusBackground, statusForeground) = GetStatusStyle(appointment.Status);
            var inProgress = appointment.Status == AppointmentStatus.InProgress;
            if (inProgress)
            {
                statusBackground = "#40FFFFFF";
                statusForeground = "#FFFFFF";
            }

            var isNext = nextAppointment is not null && appointment.Id == nextAppointment.Id;
            predictionByAppointmentId.TryGetValue(appointment.Id, out var prediction);
            var intervalLabel = FormatPredictedInterval(appointment.Status, prediction?.IntervalLabel);

            allQueueItems.Add(new QueueItemViewModel(
                appointment.Id,
                appointment.PatientId,
                appointment.QueueNumber,
                patient.Nom,
                statusLabel,
                statusBackground,
                statusForeground,
                intervalLabel,
                patient.Age,
                patient.Adresse,
                patient.Telephone,
                inProgress,
                isNext));
        }

        allPatients.Clear();
        var index = 1;
        foreach (var patient in patients)
        {
            allPatients.Add(new QueueItemViewModel(
                Guid.Empty,
                patient.Id,
                index++,
                patient.Nom,
                string.Empty,
                "#E7ECEF",
                "#2E4682",
                string.Empty,
                patient.Age,
                patient.Adresse,
                patient.Telephone,
                isSessionInProgress: false,
                isNextInQueue: false));
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

    public async Task SetInProgressAsync(QueueItemViewModel item)
    {
        if (item.AppointmentId == Guid.Empty)
        {
            return;
        }

        await appointmentService.SetInProgressAsync(item.AppointmentId);
        await ReloadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.AppointmentId == item.AppointmentId);
    }

    public async Task SetDoneAsync(QueueItemViewModel item)
    {
        if (item.AppointmentId == Guid.Empty)
        {
            return;
        }

        var doneSaved = await appointmentService.SetDoneAsync(item.AppointmentId);
        if (doneSaved)
        {
            var settingsAfterDone = await uiSettingsService.LoadAsync();
            settingsAfterDone.LastTerminatedAt = DateTime.Now;
            await uiSettingsService.SaveAsync(settingsAfterDone);
        }

        await ReloadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.AppointmentId == item.AppointmentId);
    }

    public async Task SetWaitingAsync(QueueItemViewModel item)
    {
        if (item.AppointmentId == Guid.Empty)
        {
            return;
        }

        await appointmentService.SetWaitingAsync(item.AppointmentId);
        await ReloadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.AppointmentId == item.AppointmentId);
    }

    public async Task SetCancelledAsync(QueueItemViewModel item)
    {
        if (item.AppointmentId == Guid.Empty)
        {
            return;
        }

        await appointmentService.SetCancelledAsync(item.AppointmentId);
        await ReloadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.AppointmentId == item.AppointmentId);
    }

    private static string FormatPredictedInterval(AppointmentStatus status, string? intervalLabel)
    {
        if (status is AppointmentStatus.Done or AppointmentStatus.Cancelled)
        {
            return "—";
        }

        return string.IsNullOrWhiteSpace(intervalLabel) ? "—" : intervalLabel;
    }

    private static (string Label, string Background, string Foreground) GetStatusStyle(AppointmentStatus status) => status switch
    {
        AppointmentStatus.InProgress => ("En cours", "#E7F0FF", "#1E4EAA"),
        AppointmentStatus.Done => ("Terminé", "#E6F7EA", "#237A43"),
        AppointmentStatus.Cancelled => ("Annulé", "#FCEBEC", "#B4232A"),
        _ => ("En attente", "#FFF5E6", "#9A5A00"),
    };
}

public sealed partial class QueueItemViewModel : ObservableObject
{
    public Guid AppointmentId { get; }
    public Guid PatientId { get; }
    public int Number { get; }

    [ObservableProperty]
    private string nom;

    [ObservableProperty]
    private string statusLabel;

    [ObservableProperty]
    private string statusBackground;

    [ObservableProperty]
    private string statusForeground;

    [ObservableProperty]
    private string predictedInterval;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string adresse;

    [ObservableProperty]
    private string telephone;

    /// <summary>When true, the queue card uses the app blue token as background (session en cours).</summary>
    public bool IsSessionInProgress { get; }

    /// <summary>Smallest-queue waiting patient when none en cours; otherwise first waiting after the en cours ticket.</summary>
    public bool IsNextInQueue { get; }

    public string CardBackground => IsSessionInProgress ? "#2E4682" : "Transparent";

    public string NameForeground => IsSessionInProgress ? "#FFFFFF" : "#2E4682";

    public string TimeForeground => IsSessionInProgress ? "#B8E8C4" : "#7A9E7E";

    public string QueueNumberForeground => IsSessionInProgress ? "#FFFFFF" : "#2E4682";

    public double NextAccentWidth => IsNextInQueue ? 4 : 0;

    public string NextAccentBackground => IsNextInQueue ? "#7A9E7E" : "Transparent";

    public QueueItemViewModel(
        Guid appointmentId,
        Guid patientId,
        int number,
        string nom,
        string statusLabel,
        string statusBackground,
        string statusForeground,
        string predictedInterval,
        int age,
        string adresse,
        string telephone,
        bool isSessionInProgress = false,
        bool isNextInQueue = false)
    {
        AppointmentId = appointmentId;
        PatientId = patientId;
        Number = number;
        this.nom = nom;
        this.statusLabel = statusLabel;
        this.statusBackground = statusBackground;
        this.statusForeground = statusForeground;
        this.predictedInterval = predictedInterval;
        this.age = age;
        this.adresse = adresse;
        this.telephone = telephone;
        IsSessionInProgress = isSessionInProgress;
        IsNextInQueue = isNextInQueue;
    }
}

public sealed record NewPatientInput(string Nom, int Age, string Adresse, string Telephone);

