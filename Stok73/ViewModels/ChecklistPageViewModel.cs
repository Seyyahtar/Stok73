using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace Stok73.ViewModels;

public partial class ChecklistPageViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        if (Shell.Current is null)
        {
            return;
        }

        await Shell.Current.GoToAsync("..");
    }
}
