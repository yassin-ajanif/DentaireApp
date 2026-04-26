using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentaireApp.Business.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    public const int PatientListPageSize = 20;

    public QueueViewModel Queue { get; }
    public PatientRecordViewModel PatientRecord { get; }
    private readonly IPatientRepository patientRepository;
    public ObservableCollection<QueueItemViewModel> PatientList { get; } = [];

    [ObservableProperty]
    private string patientListSearchTerm = string.Empty;

    [ObservableProperty]
    private int patientListPageIndex;

    [ObservableProperty]
    private int patientListTotalCount;

    public int PatientListPageCount =>
        Math.Max(1, (PatientListTotalCount + PatientListPageSize - 1) / PatientListPageSize);

    public string PatientListPageLabel =>
        $"{PatientListPageIndex + 1} / {PatientListPageCount} · {PatientListTotalCount} patient(s)";

    public MainWindowViewModel(
        QueueViewModel queue,
        PatientRecordViewModel patientRecord,
        IPatientRepository patientRepository)
    {
        Queue = queue;
        PatientRecord = patientRecord;
        this.patientRepository = patientRepository;
        Queue.SelectedItemChanged += OnQueueSelectedItemChangedAsync;
        Queue.QueueDataReloaded += (_, _) => _ = RefreshPatientListAsync();
    }

    /// <summary>All patients from the database for new-patient dialog autocomplete (filtered locally as the user types).</summary>
    public async Task<IReadOnlyList<PatientSuggestion>> GetNewPatientDialogSuggestionsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await patientRepository.GetAllAsync(cancellationToken);
        return items
            .Select(p => new PatientSuggestion(p.Nom, p.Telephone, p.Age, p.Adresse))
            .ToList();
    }

    public async Task InitializeAsync()
    {
        await Queue.InitializeAsync();
        await PatientRecord.LoadForPatientAsync(Queue.SelectedItem?.PatientId);
        await RefreshPatientListAsync();
    }

    private Task OnQueueSelectedItemChangedAsync(QueueItemViewModel? item)
    {
        return PatientRecord.LoadForPatientAsync(item?.PatientId);
    }

    partial void OnPatientListSearchTermChanged(string value)
    {
        PatientListPageIndex = 0;
        _ = RefreshPatientListAsync();
    }

    private async Task RefreshPatientListAsync()
    {
        var term = string.IsNullOrWhiteSpace(PatientListSearchTerm) ? null : PatientListSearchTerm.Trim();
        var skip = PatientListPageIndex * PatientListPageSize;
        var (items, total) = await patientRepository.GetPatientsPageAsync(
            skip,
            PatientListPageSize,
            term);

        PatientListTotalCount = total;
        var maxPage = Math.Max(0, (total + PatientListPageSize - 1) / PatientListPageSize - 1);
        if (PatientListPageIndex > maxPage)
        {
            PatientListPageIndex = maxPage;
            skip = PatientListPageIndex * PatientListPageSize;
            (items, total) = await patientRepository.GetPatientsPageAsync(
                skip,
                PatientListPageSize,
                term);
            PatientListTotalCount = total;
        }

        PatientList.Clear();
        var startNumber = skip + 1;
        var i = 0;
        foreach (var p in items)
        {
            PatientList.Add(new QueueItemViewModel(
                Guid.Empty,
                p.Id,
                startNumber + i,
                p.Nom,
                string.Empty,
                "#E7ECEF",
                "#2E4682",
                string.Empty,
                p.Age,
                p.Adresse,
                p.Telephone,
                isSessionInProgress: false,
                isNextInQueue: false));
            i++;
        }

        OnPropertyChanged(nameof(PatientListPageCount));
        OnPropertyChanged(nameof(PatientListPageLabel));
        PatientListNextPageCommand.NotifyCanExecuteChanged();
        PatientListPrevPageCommand.NotifyCanExecuteChanged();
    }

    private bool CanPatientListPrevPage() => PatientListPageIndex > 0;

    private bool CanPatientListNextPage()
    {
        var maxPage = Math.Max(0, (PatientListTotalCount + PatientListPageSize - 1) / PatientListPageSize - 1);
        return PatientListPageIndex < maxPage;
    }

    [RelayCommand(CanExecute = nameof(CanPatientListPrevPage))]
    private async Task PatientListPrevPageAsync()
    {
        if (PatientListPageIndex <= 0)
        {
            return;
        }

        PatientListPageIndex--;
        await RefreshPatientListAsync();
    }

    [RelayCommand(CanExecute = nameof(CanPatientListNextPage))]
    private async Task PatientListNextPageAsync()
    {
        if (!CanPatientListNextPage())
        {
            return;
        }

        PatientListPageIndex++;
        await RefreshPatientListAsync();
    }
}
