using Avalonia.Controls;
using Avalonia.Interactivity;
using DentaireApp.UI.Avalonia.ViewModels;

namespace DentaireApp.UI.Avalonia.Views;

public partial class PatientRecordDialog : Window
{
    public PatientRecordDialog()
    {
        InitializeComponent();
        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void OnOpened(object? sender, System.EventArgs e)
    {
        if (DataContext is PatientRecordViewModel vm)
        {
            vm.CloseDialog = CloseDossier;
        }
    }

    private void OnClosed(object? sender, System.EventArgs e)
    {
        if (DataContext is PatientRecordViewModel vm)
        {
            vm.CloseDialog = null;
        }
    }

    /// <summary>Same as Fermer: dismiss this window (also invoked after save confirmation OK).</summary>
    private void CloseDossier() => Close();

    private void OnFermerClick(object? sender, RoutedEventArgs e) => CloseDossier();
}
