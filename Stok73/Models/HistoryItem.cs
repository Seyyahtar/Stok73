using System;
using Microsoft.Maui.Graphics;

namespace Stok73.Models;

public sealed class HistoryItem
{
    public HistoryItem(string type, string title, string detail, DateTime date, Color badgeColor, Color badgeText)
    {
        Type = type;
        Title = title;
        Detail = detail;
        Date = date;
        BadgeColor = badgeColor;
        BadgeTextColor = badgeText;
    }

    public string Type { get; }
    public string Title { get; }
    public string Detail { get; }
    public DateTime Date { get; }
    public Color BadgeColor { get; }
    public Color BadgeTextColor { get; }
}
