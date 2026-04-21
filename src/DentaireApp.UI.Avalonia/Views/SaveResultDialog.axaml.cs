using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DentaireApp.UI.Avalonia.Views;

public partial class SaveResultDialog : Window
{
    public SaveResultDialog()
    {
        InitializeComponent();
    }

    public SaveResultDialog(bool isSuccess, string message)
    {
        InitializeComponent();
        Title = isSuccess ? "Enregistrement reussi" : "Echec d'enregistrement";
        MessageTextBlock.Text = message;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

