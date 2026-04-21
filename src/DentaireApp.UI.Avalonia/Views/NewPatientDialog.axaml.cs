using Avalonia.Controls;
using Avalonia.Interactivity;
using DentaireApp.UI.Avalonia.ViewModels;
using System.Linq;

namespace DentaireApp.UI.Avalonia.Views;

public partial class NewPatientDialog : Window
{
    public NewPatientDialog()
    {
        InitializeComponent();
    }

    public NewPatientDialog(NewPatientInput currentPatient, string title)
        : this()
    {
        Title = title;
        SaveButton.Content = "Modifier";
        NomTextBox.Text = currentPatient.Nom;
        AgeTextBox.Text = currentPatient.Age > 0 ? currentPatient.Age.ToString() : string.Empty;
        AdresseTextBox.Text = currentPatient.Adresse;
        TelephoneTextBox.Text = currentPatient.Telephone;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var telephone = TelephoneTextBox.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(NomTextBox.Text) ||
            string.IsNullOrWhiteSpace(telephone))
        {
            FeedbackTextBlock.Text = "Nom et Telephone sont obligatoires.";
            return;
        }

        if (telephone.Any(char.IsLetter))
        {
            FeedbackTextBlock.Text = "Telephone ne doit pas contenir de lettres.";
            return;
        }

        var age = 0;
        if (!string.IsNullOrWhiteSpace(AgeTextBox.Text) &&
            (!int.TryParse(AgeTextBox.Text, out age) || age <= 0))
        {
            FeedbackTextBlock.Text = "Age doit etre un nombre positif.";
            return;
        }

        FeedbackTextBlock.Text = string.Empty;

        var input = new NewPatientInput(
            NomTextBox.Text.Trim(),
            age,
            AdresseTextBox.Text?.Trim() ?? string.Empty,
            telephone);

        Close(input);
    }
}

