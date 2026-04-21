using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

    public Func<Task<NewPatientInput?>>? RequestNewPatientAsync { get; set; }

    public ObservableCollection<QueueItemViewModel> Items { get; } =
    [
        new QueueItemViewModel(21, "ahmed ajanif", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(22, "ahmed ajanif", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(23, "yassin ajanif", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(24, "karim benali", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(25, "fatima el amrani", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(26, "omar idrissi", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(27, "sanae mouline", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(28, "mehdi chraibi", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(29, "nadia zerhouni", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(30, "youssef tazi", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(31, "houda filali", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(32, "rachid bensaid", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(33, "imane loukili", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(34, "anass harouch", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(35, "salma benjelloun", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(36, "hamza ouazzani", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(37, "laila sekkat", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(38, "bilal kettani", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(39, "khadija ramdani", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(40, "amine fassi", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(41, "zineb alaoui", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(42, "tarik benkirane", "15:30 - 16:15", 0, string.Empty, string.Empty),
        new QueueItemViewModel(43, "hanae mazouz", "15:30 - 16:15", 0, string.Empty, string.Empty),
    ];

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

        var nextNumber = Items.Count == 0 ? 1 : Items.Max(x => x.Number) + 1;
        var newItem = new QueueItemViewModel(
            nextNumber,
            input.Nom.Trim(),
            "Pending doctor arrival",
            input.Age,
            input.Adresse,
            input.Telephone);
        Items.Add(newItem);
        SelectedItem = newItem;
    }

    public void UpdatePatientCredentials(QueueItemViewModel item, NewPatientInput input)
    {
        item.Nom = input.Nom.Trim();
        item.Age = input.Age;
        item.Adresse = input.Adresse.Trim();
        item.Telephone = input.Telephone.Trim();
        SelectedItem = item;
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
}

public sealed partial class QueueItemViewModel : ObservableObject
{
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

    public QueueItemViewModel(int number, string nom, string predictedInterval, int age, string adresse, string telephone)
    {
        Number = number;
        this.nom = nom;
        this.predictedInterval = predictedInterval;
        this.age = age;
        this.adresse = adresse;
        this.telephone = telephone;
    }
}

public sealed record NewPatientInput(string Nom, int Age, string Adresse, string Telephone);

