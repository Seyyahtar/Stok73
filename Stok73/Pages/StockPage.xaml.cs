using Microsoft.Maui.Controls;
using Stok73.ViewModels;

namespace Stok73.Pages;

public partial class StockPage : ContentPage
{
    private readonly StockPageViewModel _viewModel;

    public StockPage(StockPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.InitializeAsync();
    }
}
