using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DentaireApp.UI.Avalonia.Views;

public partial class ConfirmDeletePatientDialog : Window
{
    public ConfirmDeletePatientDialog()
    {
        InitializeComponent();
    }

    public ConfirmDeletePatientDialog(string patientNom)
        : this()
    {
        MessageTextBlock.Text =
            $"Supprimer définitivement « {patientNom} », ses rendez-vous et son historique de soins ?";
    }

    private void OnYesClick(object? sender, RoutedEventArgs e) => Close(true);

    private void OnNoClick(object? sender, RoutedEventArgs e) => Close(false);
}
