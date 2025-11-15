using System.Collections.Generic;

namespace Stok73.Models;

public class StockGroup
{
    public StockGroup(string title, int totalQuantity, IReadOnlyList<StockItem> items)
    {
        Title = title;
        TotalQuantity = totalQuantity;
        Items = items;
    }

    public string Title { get; }

    public int TotalQuantity { get; }

    public IReadOnlyList<StockItem> Items { get; }
}
