using CommunityToolkit.Mvvm.ComponentModel;

namespace Stok73.Models;

public partial class CaseMaterialEntry : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string serialLotNumber = string.Empty;

    [ObservableProperty]
    private string ubbCode = string.Empty;

    [ObservableProperty]
    private string quantity = string.Empty;
}
