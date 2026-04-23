using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Models.Patients;
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
    private int age;

    [ObservableProperty]
    private string telephone = string.Empty;

    [ObservableProperty]
    private string adresse = string.Empty;

    [ObservableProperty]
    private string saveMessage = string.Empty;

    public Func<bool, string, Task>? ShowSaveResultAsync { get; set; }

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

        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Telephone))
        {
            SaveMessage = "Nom et Telephone sont obligatoires.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

        if (Telephone.Any(char.IsLetter))
        {
            SaveMessage = "Echec: Telephone ne doit pas contenir de lettres.";
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

        patient.Nom = Nom.Trim();
        patient.Age = Age;
        patient.Telephone = Telephone.Trim();
        patient.Adresse = Adresse.Trim();
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
    }

    [RelayCommand]
    private void Annuler()
    {
        SaveMessage = string.Empty;
    }

    [RelayCommand]
    private void AddTreatmentInfo()
    {
        TreatmentInfos.Add(new TreatmentInfoViewModel
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Today,
            NatureOperation = string.Empty,
            PrixConven = 0m,
            Recu = 0m,
            ARecevoir = 0m,
        });
    }

    public async Task LoadForPatientAsync(Guid? patientId)
    {
        currentPatientId = patientId;

        TreatmentInfos.Clear();
        SaveMessage = string.Empty;

        if (patientId is null)
        {
            Nom = string.Empty;
            Age = 0;
            Telephone = string.Empty;
            Adresse = string.Empty;
            return;
        }

        var patient = await patientRecordService.GetPatientRecordAsync(patientId.Value);
        if (patient is null)
        {
            Nom = string.Empty;
            Age = 0;
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

