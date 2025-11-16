using Microsoft.Maui.Controls;
using Stok73.Pages;

namespace Stok73;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("stock", typeof(StockPage));
        Routing.RegisterRoute("case", typeof(CasePage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("checklist", typeof(ChecklistPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
    }
}
