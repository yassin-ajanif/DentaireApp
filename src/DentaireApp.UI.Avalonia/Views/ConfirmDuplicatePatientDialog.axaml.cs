using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DentaireApp.UI.Avalonia.Views;

public partial class ConfirmDuplicatePatientDialog : Window
{
    public ConfirmDuplicatePatientDialog()
    {
        InitializeComponent();
    }

    public ConfirmDuplicatePatientDialog(string existingPatientNom)
        : this()
    {
        PatientNameRun.Text = existingPatientNom;
    }

    private void OnYesClick(object? sender, RoutedEventArgs e) =>
        Close(true);

    private void OnNoClick(object? sender, RoutedEventArgs e) =>
        Close(false);
}
