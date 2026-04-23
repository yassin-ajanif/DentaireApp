using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public QueueViewModel Queue { get; }
    public PatientRecordViewModel PatientRecord { get; }
    public System.Collections.ObjectModel.ReadOnlyObservableCollection<QueueItemViewModel> PatientList { get; }

    public MainWindowViewModel(QueueViewModel queue, PatientRecordViewModel patientRecord)
    {
        Queue = queue;
        PatientRecord = patientRecord;
        PatientList = queue.AllPatients;
        Queue.SelectedItemChanged += OnQueueSelectedItemChangedAsync;
    }

    public async Task InitializeAsync()
    {
        await Queue.InitializeAsync();
        await PatientRecord.LoadForPatientAsync(Queue.SelectedItem?.PatientId);
    }

    private Task OnQueueSelectedItemChangedAsync(QueueItemViewModel? item)
    {
        return PatientRecord.LoadForPatientAsync(item?.PatientId);
    }
}
