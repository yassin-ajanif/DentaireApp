using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public QueueViewModel Queue { get; }
    public PatientRecordViewModel PatientRecord { get; }
    private readonly ReadOnlyObservableCollection<QueueItemViewModel> allPatients;
    public ObservableCollection<QueueItemViewModel> PatientList { get; } = [];

    [ObservableProperty]
    private string patientListSearchTerm = string.Empty;

    public MainWindowViewModel(QueueViewModel queue, PatientRecordViewModel patientRecord)
    {
        Queue = queue;
        PatientRecord = patientRecord;
        allPatients = queue.AllPatients;
        Queue.SelectedItemChanged += OnQueueSelectedItemChangedAsync;
        ((INotifyCollectionChanged)allPatients).CollectionChanged += OnAllPatientsCollectionChanged;
        ApplyPatientListFilter();
    }

    public async Task InitializeAsync()
    {
        await Queue.InitializeAsync();
        await PatientRecord.LoadForPatientAsync(Queue.SelectedItem?.PatientId);
        ApplyPatientListFilter();
    }

    private Task OnQueueSelectedItemChangedAsync(QueueItemViewModel? item)
    {
        return PatientRecord.LoadForPatientAsync(item?.PatientId);
    }

    partial void OnPatientListSearchTermChanged(string value) => ApplyPatientListFilter();

    private void OnAllPatientsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        ApplyPatientListFilter();

    private void ApplyPatientListFilter()
    {
        var term = PatientListSearchTerm?.Trim();
        var filtered = string.IsNullOrWhiteSpace(term)
            ? allPatients
            : allPatients.Where(x =>
                x.Nom.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Telephone.Contains(term, StringComparison.OrdinalIgnoreCase));

        PatientList.Clear();
        foreach (var patient in filtered)
        {
            PatientList.Add(patient);
        }
    }
}
