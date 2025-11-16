using Microsoft.Maui.Controls;

namespace Stok73.Pages;

public partial class CasePage : ContentPage
{
    public CasePage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.CasePageViewModel();
    }
}
