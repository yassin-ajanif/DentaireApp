using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using DentaireApp.UI.Avalonia.Services;
using DentaireApp.UI.Avalonia.ViewModels;
using System;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly UiSettingsFileService uiSettingsService = new();

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
        if (DataContext is not MainWindowViewModel vm)
        {
            return null;
        }

        var suggestions = await vm.GetNewPatientDialogSuggestionsAsync();

        var dialog = new NewPatientDialog(suggestions);
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

    private async void OnQueueListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBox || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var item = FindQueueItemDataContext(e.Source as Control);
        if (item is null)
        {
            return;
        }

        vm.Queue.SelectedItem = item;
        e.Handled = true;
        await ShowPatientRecordDialogAsync(item.PatientId);
    }

    private async void OnQueueDossierPatientClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is null)
        {
            return;
        }

        await ShowPatientRecordDialogAsync(vm.Queue.SelectedItem.PatientId);
    }

    private async void OnQueueSetInProgressClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        await vm.Queue.SetInProgressAsync(item);
    }

    private async void OnQueueSetDoneClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        await vm.Queue.SetDoneAsync(item);
    }

    private async void OnQueueSetWaitingClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        await vm.Queue.SetWaitingAsync(item);
    }

    private async void OnQueueSetCancelledClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm || vm.Queue.SelectedItem is not QueueItemViewModel item)
        {
            return;
        }

        await vm.Queue.SetCancelledAsync(item);
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

    private async void OnPatientListDataGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not DataGrid)
        {
            return;
        }

        var item = FindQueueItemDataContext(e.Source as Control);
        if (item is null)
        {
            return;
        }

        PatientListDataGrid.SelectedItem = item;
        e.Handled = true;
        await ShowPatientRecordDialogAsync(item.PatientId);
    }

    private async void OnPatientListDeletePatientClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm ||
            PatientListDataGrid.SelectedItem is not QueueItemViewModel item ||
            item.PatientId == Guid.Empty)
        {
            return;
        }

        var confirm = new ConfirmDeletePatientDialog(item.Nom);
        if (!await confirm.ShowDialog<bool>(this))
        {
            return;
        }

        try
        {
            var ok = await vm.Queue.TryDeletePatientAsync(item.PatientId);
            if (!ok)
            {
                await ShowSaveResultDialogAsync(false, "Patient introuvable.", this);
                return;
            }

            await vm.PatientRecord.ClearIfCurrentPatientAsync(item.PatientId);
        }
        catch (Exception ex)
        {
            await ShowSaveResultDialogAsync(false, $"Échec de la suppression : {ex.Message}", this);
        }
    }

    private async void OnOpenSettingsClick(object? sender, RoutedEventArgs e)
    {
        var settings = await uiSettingsService.LoadAsync();
        var dialog = new SettingsDialog(settings);
        var updated = await dialog.ShowDialog<UiSettings?>(this);
        if (updated is not null)
        {
            await uiSettingsService.SaveAsync(updated);
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.Queue.InitializeAsync();
            }
        }
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

        await vm.Queue.InitializeAsync();
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