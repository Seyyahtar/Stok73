using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Stok73.Models;

public partial class FilterOption : ObservableObject
{
    public FilterOption(string key, string label, Color accentColor)
    {
        Key = key;
        Label = label;
        AccentColor = accentColor;

        BackgroundColor = Colors.Transparent;
        TextColor = Color.FromArgb("#1F2937");
        IndicatorColor = accentColor;
        StrokeColor = accentColor;
    }

    public string Key { get; }

    public string Label { get; }

    public Color AccentColor { get; }

    [ObservableProperty]
    private bool isActive;

    [ObservableProperty]
    private Color backgroundColor;

    [ObservableProperty]
    private Color textColor;

    [ObservableProperty]
    private Color indicatorColor;

    [ObservableProperty]
    private Color strokeColor;

    partial void OnIsActiveChanged(bool value)
    {
        if (value)
        {
            BackgroundColor = AccentColor;
            TextColor = Colors.White;
            IndicatorColor = Colors.White;
            StrokeColor = Colors.Transparent;
        }
        else
        {
            BackgroundColor = Colors.Transparent;
            TextColor = Color.FromArgb("#1F2937");
            IndicatorColor = AccentColor;
            StrokeColor = AccentColor;
        }
    }
}

