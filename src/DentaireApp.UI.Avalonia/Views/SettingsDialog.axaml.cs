using Avalonia.Controls;
using Avalonia.Interactivity;
using DentaireApp.UI.Avalonia.Services;

namespace DentaireApp.UI.Avalonia.Views;

public partial class SettingsDialog : Window
{
    private readonly UiSettings seedSettings;

    public SettingsDialog()
    {
        InitializeComponent();
        seedSettings = new UiSettings();
    }

    public SettingsDialog(UiSettings currentSettings)
        : this()
    {
        seedSettings = new UiSettings
        {
            AverageTimeSpentWithPatient = currentSettings.AverageTimeSpentWithPatient,
            LastTerminatedAt = currentSettings.LastTerminatedAt,
        };
        AverageTimeTextBox.Text = currentSettings.AverageTimeSpentWithPatient.ToString();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (!int.TryParse(AverageTimeTextBox.Text?.Trim(), out var minutes) || minutes <= 0)
        {
            FeedbackTextBlock.Text = "Veuillez saisir un entier positif.";
            return;
        }

        FeedbackTextBlock.Text = string.Empty;
        Close(new UiSettings
        {
            AverageTimeSpentWithPatient = minutes,
            LastTerminatedAt = seedSettings.LastTerminatedAt,
        });
    }
}
