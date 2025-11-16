using Microsoft.Maui.Controls;

namespace Stok73.Pages;

public partial class ChecklistPage : ContentPage
{
    public ChecklistPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.ChecklistPageViewModel();
    }
}
