using Microsoft.Maui.Graphics;

namespace Stok73.Models;

public sealed class DeviceCategorySummary
{
    public DeviceCategorySummary(string title, string description, int quantity, Color badgeBackground, Color badgeText)
    {
        Title = title;
        Description = description;
        Quantity = quantity;
        BadgeBackground = badgeBackground;
        BadgeText = badgeText;
    }

    public string Title { get; }

    public string Description { get; }

    public int Quantity { get; }

    public Color BadgeBackground { get; }

    public Color BadgeText { get; }
}
