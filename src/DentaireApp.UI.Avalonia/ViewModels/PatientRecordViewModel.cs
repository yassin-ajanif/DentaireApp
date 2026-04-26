using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Patients;
using DentaireApp.Business.Validation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public partial class PatientRecordViewModel : ViewModelBase
{
    private readonly IPatientRepository patientRepository;
    private readonly IPatientRecordService patientRecordService;
    private Guid? currentPatientId;

    [ObservableProperty]
    private string nom = string.Empty;

    [ObservableProperty]
    private int? age;

    [ObservableProperty]
    private string telephone = string.Empty;

    [ObservableProperty]
    private string adresse = string.Empty;

    [ObservableProperty]
    private string saveMessage = string.Empty;

    [ObservableProperty]
    private TreatmentInfoViewModel? selectedTreatmentInfo;

    public Func<bool, string, Task>? ShowSaveResultAsync { get; set; }

    /// <summary>Same effect as closing the dossier window (Fermer); set by the patient record window code-behind.</summary>
    public Action? CloseDialog { get; set; }

    public ObservableCollection<TreatmentInfoViewModel> TreatmentInfos { get; } = [];

    public PatientRecordViewModel(IPatientRepository patientRepository, IPatientRecordService patientRecordService)
    {
        this.patientRepository = patientRepository;
        this.patientRecordService = patientRecordService;
    }

    [RelayCommand]
    private async Task Enregistrer()
    {
        if (currentPatientId is null)
        {
            SaveMessage = "Aucun patient selectionne.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(Nom))
        {
            SaveMessage = "Le nom est obligatoire.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        var normalizedName = Nom.Trim();
        if (normalizedName.Length > PatientValidation.MaxNameLength)
        {
            SaveMessage = $"Le nom ne doit pas dépasser {PatientValidation.MaxNameLength} caractères.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        if (!TelephoneValidation.TryNormalizeMorocco(Telephone, out var normalizedPhone, out var phoneError))
        {
            SaveMessage = phoneError;
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        var patient = await patientRepository.GetByIdAsync(currentPatientId.Value);
        if (patient is null)
        {
            SaveMessage = "Patient introuvable.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        patient.Nom = normalizedName;
        patient.Age = Age;
        patient.Telephone = normalizedPhone;
        patient.Adresse = Adresse.Trim();
        Telephone = normalizedPhone;
        await patientRepository.UpdateAsync(patient);

        var infos = TreatmentInfos.Select(line => new TreatmentInfo
        {
            Id = line.Id,
            PatientId = currentPatientId.Value,
            Date = line.Date,
            NatureOperation = line.NatureOperation.Trim(),
            PrixConven = line.PrixConven,
            Recu = line.Recu,
            ARecevoir = line.ARecevoir,
        }).ToList();

        await patientRecordService.SaveTreatmentInfosAsync(currentPatientId.Value, infos);

        SaveMessage = "Enregistrement effectue.";
        if (ShowSaveResultAsync is not null)
        {
            await ShowSaveResultAsync(true, SaveMessage);
        }

        CloseDialog?.Invoke();
    }

    [RelayCommand]
    private void Annuler()
    {
        SaveMessage = string.Empty;
        CloseDialog?.Invoke();
    }

    [RelayCommand]
    private void AddTreatmentInfo()
    {
        TreatmentInfos.Add(new TreatmentInfoViewModel
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now,
            NatureOperation = string.Empty,
            PrixConven = 0m,
            Recu = 0m,
            ARecevoir = 0m,
        });
    }

    [RelayCommand]
    private void DeleteSelectedTreatmentInfo()
    {
        DeleteTreatmentInfo(SelectedTreatmentInfo);
    }

    public void DeleteTreatmentInfo(TreatmentInfoViewModel? treatmentInfo)
    {
        if (treatmentInfo is null)
        {
            return;
        }

        TreatmentInfos.Remove(treatmentInfo);
        if (ReferenceEquals(SelectedTreatmentInfo, treatmentInfo))
        {
            SelectedTreatmentInfo = null;
        }
    }

    public async Task LoadForPatientAsync(Guid? patientId)
    {
        currentPatientId = patientId;

        TreatmentInfos.Clear();
        SaveMessage = string.Empty;

        if (patientId is null)
        {
            Nom = string.Empty;
            Age = null;
            Telephone = string.Empty;
            Adresse = string.Empty;
            return;
        }

        var patient = await patientRecordService.GetPatientRecordAsync(patientId.Value);
        if (patient is null)
        {
            Nom = string.Empty;
            Age = null;
            Telephone = string.Empty;
            Adresse = string.Empty;
            return;
        }

        Nom = patient.Nom;
        Age = patient.Age;
        Telephone = patient.Telephone;
        Adresse = patient.Adresse;

        var infos = await patientRecordService.GetTreatmentInfosAsync(patientId.Value);
        foreach (var info in infos.OrderBy(x => x.Date))
        {
            TreatmentInfos.Add(new TreatmentInfoViewModel
            {
                Id = info.Id,
                Date = info.Date,
                NatureOperation = info.NatureOperation,
                PrixConven = info.PrixConven,
                Recu = info.Recu,
                ARecevoir = info.ARecevoir,
            });
        }
    }

    public Task ClearIfCurrentPatientAsync(Guid patientId) =>
        currentPatientId == patientId ? LoadForPatientAsync(null) : Task.CompletedTask;
}

public sealed partial class TreatmentInfoViewModel : ObservableObject
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private DateTime date;

    [ObservableProperty]
    private string natureOperation = string.Empty;

    [ObservableProperty]
    private decimal prixConven;

    [ObservableProperty]
    private decimal recu;

    [ObservableProperty]
    private decimal aRecevoir;
}

