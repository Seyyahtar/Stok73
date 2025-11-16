using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace Stok73.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
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

    [RelayCommand]
    private async Task ClearStocksAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert("Bilgi", "Stok kayıtlarını temizleme işlemi yakında eklenecek.", "Tamam");
        }
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert("Bilgi", "Geçmiş kayıtlarını temizleme işlemi yakında eklenecek.", "Tamam");
        }
    }

    [RelayCommand]
    private async Task ClearAllAsync()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert("Bilgi", "Tüm verileri temizleme işlemi yakında eklenecek.", "Tamam");
        }
    }
}
