using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using DentaireApp.UI.Avalonia.ViewModels;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.Queue.RequestNewPatientAsync = ShowNewPatientDialogAsync;
            vm.Queue.ConfirmDuplicatePatientEnqueueAsync = ShowConfirmDuplicatePatientDialogAsync;
            vm.PatientRecord.ShowSaveResultAsync = ShowSaveResultDialogAsync;
            _ = vm.InitializeAsync();
        }
    }

    private async Task<NewPatientInput?> ShowNewPatientDialogAsync()
    {
        var dialog = new NewPatientDialog();
        return await dialog.ShowDialog<NewPatientInput?>(this);
    }

    private async Task<bool> ShowConfirmDuplicatePatientDialogAsync(string existingPatientNom)
    {
        var dialog = new ConfirmDuplicatePatientDialog(existingPatientNom);
        return await dialog.ShowDialog<bool>(this);
    }

    private async Task ShowSaveResultDialogAsync(bool isSuccess, string message)
    {
        var dialog = new SaveResultDialog(isSuccess, message);
        await dialog.ShowDialog(this);
    }

    private async void OnEditPatientCredentialsClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        var currentPatient = new NewPatientInput(item.Nom, item.Age, item.Adresse, item.Telephone);
        var dialog = new NewPatientDialog(currentPatient, "Modifier informations patient");
        var updated = await dialog.ShowDialog<NewPatientInput?>(this);
        if (updated is null)
        {
            return;
        }

        await vm.Queue.UpdatePatientCredentialsAsync(item, updated);
    }

    private void OnQueueItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm ||
            e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed ||
            sender is not Control { DataContext: QueueItemViewModel item })
        {
            return;
        }

        vm.Queue.SelectedItem = item;
    }
}