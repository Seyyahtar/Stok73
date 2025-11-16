using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Stok73.Models;

public partial class StockGroup : ObservableObject
{
    public StockGroup(string title, IReadOnlyList<StockMaterialGroup> materials)
    {
        Title = title;
        Materials = materials;
        TotalQuantity = materials.Sum(material => material.TotalQuantity);
        IsExpanded = true;
    }

    public string Title { get; }

    public int TotalQuantity { get; }

    public IReadOnlyList<StockMaterialGroup> Materials { get; }

    [ObservableProperty]
    private bool isExpanded;
}
