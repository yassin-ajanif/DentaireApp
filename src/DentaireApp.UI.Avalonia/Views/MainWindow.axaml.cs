using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using DentaireApp.UI.Avalonia.ViewModels;
using System;
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
            vm.PatientRecord.ShowSaveResultAsync = (ok, msg) => ShowSaveResultDialogAsync(ok, msg, this);
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

    private static async Task ShowSaveResultDialogAsync(bool isSuccess, string message, Window owner)
    {
        var dialog = new SaveResultDialog(isSuccess, message);
        await dialog.ShowDialog(owner);
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

    private async void OnQueueDossierPatientClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is null)
        {
            return;
        }

        await ShowPatientRecordDialogAsync(vm.Queue.SelectedItem.PatientId);
    }

    private void OnPatientListDataGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(PatientListDataGrid).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
        {
            return;
        }

        var item = FindQueueItemDataContext(e.Source as Control);
        if (item is not null)
        {
            PatientListDataGrid.SelectedItem = item;
        }
    }

    private async void OnPatientListDossierPatientClick(object? sender, RoutedEventArgs e)
    {
        if (PatientListDataGrid.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        await ShowPatientRecordDialogAsync(item.PatientId);
    }

    private async Task ShowPatientRecordDialogAsync(Guid patientId)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        await vm.PatientRecord.LoadForPatientAsync(patientId);
        var dossierDialog = new PatientRecordDialog { DataContext = vm.PatientRecord };
        var previousSaveFeedback = vm.PatientRecord.ShowSaveResultAsync;
        vm.PatientRecord.ShowSaveResultAsync = (ok, msg) => ShowSaveResultDialogAsync(ok, msg, dossierDialog);
        try
        {
            await dossierDialog.ShowDialog(this);
        }
        finally
        {
            vm.PatientRecord.ShowSaveResultAsync = previousSaveFeedback;
        }
    }

    private static QueueItemViewModel? FindQueueItemDataContext(Control? control)
    {
        while (control is not null)
        {
            if (control.DataContext is QueueItemViewModel q)
            {
                return q;
            }

            control = control.Parent as Control;
        }

        return null;
    }
}