using Avalonia.Controls;
using Avalonia.Interactivity;
using DentaireApp.Business.Validation;
using DentaireApp.UI.Avalonia.ViewModels;

namespace DentaireApp.UI.Avalonia.Views;

public partial class NewPatientDialog : Window
{
    public NewPatientDialog()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var name = NomTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            FeedbackTextBlock.Text = "Le nom est obligatoire.";
            return;
        }

        if (name.Length > PatientValidation.MaxNameLength)
        {
            FeedbackTextBlock.Text = $"Le nom ne doit pas dépasser {PatientValidation.MaxNameLength} caractères.";
            return;
        }

        var telephoneRaw = TelephoneTextBox.Text ?? string.Empty;
        if (!TelephoneValidation.TryNormalizeMorocco(telephoneRaw, out var telephone, out var phoneError))
        {
            FeedbackTextBlock.Text = phoneError;
            return;
        }

        int? age = null;
        if (!string.IsNullOrWhiteSpace(AgeTextBox.Text))
        {
            if (!int.TryParse(AgeTextBox.Text.Trim(), out var parsed) || parsed <= 0)
            {
                FeedbackTextBlock.Text = "L'âge doit être un nombre positif.";
                return;
            }

            age = parsed;
        }

        FeedbackTextBlock.Text = string.Empty;

        var input = new NewPatientInput(
            name,
            age,
            AdresseTextBox.Text?.Trim() ?? string.Empty,
            telephone);

        Close(input);
    }
}

