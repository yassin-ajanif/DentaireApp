using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public partial class PatientRecordViewModel : ViewModelBase
{
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

    public List<TreatmentLineViewModel> Lines { get; } =
    [
        new(DateTime.Today, "this is a test data", 100m, 60m, 40m),
    ];

    [RelayCommand]
    private async Task Enregistrer()
    {
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

        if (Lines.Any(line => line.PrixConven != line.Recu + line.ARecevoir))
        {
            SaveMessage = "Echec: Prix Conven doit etre egal a Recu + A Recevoir.";
            if (ShowSaveResultAsync is not null)
            {
                await ShowSaveResultAsync(false, SaveMessage);
            }
            return;
        }

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
    private void AddSheet()
    {
        Lines.Add(new TreatmentLineViewModel(DateTime.Today, string.Empty, 0m, 0m, 0m));
        OnPropertyChanged(nameof(Lines));
    }
}

public sealed record TreatmentLineViewModel(
    DateTime Date,
    string NatureOperation,
    decimal PrixConven,
    decimal Recu,
    decimal ARecevoir);

