using Microsoft.Maui.Controls;

namespace Stok73.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.SettingsPageViewModel();
    }
}
