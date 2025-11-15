using Microsoft.Maui.Controls;
using Stok73.Pages;

namespace Stok73;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("stock", typeof(StockPage));
    }
}
