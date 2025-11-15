using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Stok73.Models;

namespace Stok73.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IReadOnlyDictionary<string, string> _defaultRoutes = new Dictionary<string, string>
    {
        ["stock"] = "stock",
        ["case-entry"] = "case-entry",
        ["checklist"] = "checklist",
        ["history"] = "history",
        ["settings"] = "settings",
    };

    public ObservableCollection<HomeMenuItem> PrimaryMenuItems { get; } = new();
    public ObservableCollection<HomeMenuItem> SecondaryMenuItems { get; } = new();

    [ObservableProperty]
    private string currentUser = "Dr. Demir";

    public MainPageViewModel()
    {
        InitializeMenuItems();
    }

    private void InitializeMenuItems()
    {
        PrimaryMenuItems.Clear();
        SecondaryMenuItems.Clear();

        PrimaryMenuItems.Add(new HomeMenuItem(
            "stock",
            "Stok",
            Fonts.FluentUI.cube_24_regular,
            Color.FromArgb("#3B82F6"),
            _defaultRoutes["stock"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "case-entry",
            "Vaka",
            Fonts.FluentUI.clipboard_24_regular,
            Color.FromArgb("#22C55E"),
            _defaultRoutes["case-entry"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "checklist",
            "Kontrol Listesi",
            Fonts.FluentUI.checkmark_circle_24_regular,
            Color.FromArgb("#A855F7"),
            _defaultRoutes["checklist"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "history",
            "Geçmiş",
            Fonts.FluentUI.history_24_regular,
            Color.FromArgb("#F97316"),
            _defaultRoutes["history"]));

        SecondaryMenuItems.Add(new HomeMenuItem(
            "settings",
            "Ayarlar",
            Fonts.FluentUI.settings_24_regular,
            Color.FromArgb("#6B7280"),
            _defaultRoutes["settings"]));
    }

    [RelayCommand]
    private async Task OpenMenuAsync(HomeMenuItem? item)
    {
        if (item is null || Shell.Current is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(item.Route))
        {
            await Shell.Current.DisplayAlert("Bilgi", $"{item.Title} sayfası yakında eklenecek.", "Tamam");
            return;
        }

        // Şimdilik sayfalar hazır olmadığı için kullanıcıyı bilgilendiriyoruz.
        await Shell.Current.DisplayAlert("Bilgi", $"{item.Title} sayfası yakında eklenecek.", "Tamam");
    }
}
