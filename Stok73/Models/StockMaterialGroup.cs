using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Stok73.Models;

public partial class StockMaterialGroup : ObservableObject
{
    public StockMaterialGroup(string fullName, string subtitle, IReadOnlyList<StockItem> items)
    {
        FullName = fullName;
        Subtitle = subtitle;
        Items = items;
        TotalQuantity = items.Sum(item => item.Quantity);
        IsExpanded = true;
    }

    public string FullName { get; }

    public string Subtitle { get; }

    public int TotalQuantity { get; }

    public IReadOnlyList<StockItem> Items { get; }

    [ObservableProperty]
    private bool isExpanded;
}
