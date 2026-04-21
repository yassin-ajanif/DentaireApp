namespace DentaireApp.UI.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public QueueViewModel Queue { get; }
    public PatientRecordViewModel PatientRecord { get; }

    public MainWindowViewModel(QueueViewModel queue, PatientRecordViewModel patientRecord)
    {
        Queue = queue;
        PatientRecord = patientRecord;
    }
}
