using DentaireApp.UI.Avalonia.ViewModels;

namespace DentaireApp.UI.Tests;

public class UnitTest1
{
    [Fact]
    public void QueueViewModel_NavigatesToNextItem()
    {
        var vm = new QueueViewModel();
        vm.SuivantCommand.Execute(null);
        Assert.NotNull(vm.SelectedItem);
    }
}
