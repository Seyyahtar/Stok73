using Microsoft.Maui.Controls;

namespace Stok73.Pages;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.HistoryPageViewModel();
    }
}
