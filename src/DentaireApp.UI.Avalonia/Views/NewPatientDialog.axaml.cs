using Avalonia.Controls;
using Avalonia.Interactivity;
using DentaireApp.Business.Validation;
using DentaireApp.UI.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DentaireApp.UI.Avalonia.Views;

public partial class NewPatientDialog : Window
{
    private readonly List<PatientSuggestion> allSuggestions;

    public NewPatientDialog()
        : this([])
    {
    }

    public NewPatientDialog(IEnumerable<PatientSuggestion> suggestions)
    {
        InitializeComponent();
        allSuggestions = suggestions
            .Where(x => !string.IsNullOrWhiteSpace(x.Nom))
            .OrderBy(x => x.Nom, StringComparer.OrdinalIgnoreCase)
            .ToList();
        UpdateSuggestions();
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

    private void OnNameOrPhoneChanged(object? sender, TextChangedEventArgs e) => UpdateSuggestions();

    private void OnSuggestionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SuggestionsListBox.SelectedItem is not PatientSuggestion selected)
        {
            return;
        }

        NomTextBox.Text = selected.Nom;
        TelephoneTextBox.Text = selected.Telephone;
        AgeTextBox.Text = selected.Age?.ToString() ?? string.Empty;
        AdresseTextBox.Text = selected.Adresse;
        SuggestionsListBox.SelectedItem = null;
        SuggestionsListBox.IsVisible = false;
        FeedbackTextBlock.Text = "Patient existant détecté. Vérifiez avant d'enregistrer.";
    }

    private void UpdateSuggestions()
    {
        if (allSuggestions.Count == 0)
        {
            SuggestionsListBox.IsVisible = false;
            return;
        }

        var nameTerm = NomTextBox.Text?.Trim() ?? string.Empty;
        var phoneTerm = TelephoneTextBox.Text?.Trim() ?? string.Empty;
        if (nameTerm.Length < 2 && phoneTerm.Length < 3)
        {
            SuggestionsListBox.ItemsSource = null;
            SuggestionsListBox.IsVisible = false;
            return;
        }

        var filtered = allSuggestions
            .Where(x =>
                (nameTerm.Length >= 2 && x.Nom.Contains(nameTerm, StringComparison.OrdinalIgnoreCase)) ||
                (phoneTerm.Length >= 3 && x.Telephone.Contains(phoneTerm, StringComparison.OrdinalIgnoreCase)))
            .Take(6)
            .ToList();

        SuggestionsListBox.ItemsSource = filtered;
        SuggestionsListBox.IsVisible = filtered.Count > 0;
    }
}

