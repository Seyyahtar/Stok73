using Microsoft.Maui.Controls;
using Stok73.ViewModels;

namespace Stok73.Pages;

public partial class StockPage : ContentPage
{
    public StockPage(StockPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
