using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
        ["case-entry"] = "case",
        ["checklist"] = "checklist",
        ["history"] = "history",
        ["settings"] = "settings",
    };

    public ObservableCollection<HomeMenuItem> PrimaryMenuItems { get; } = new();

    [ObservableProperty]
    private string currentUser = "Dr. Demir";

    public MainPageViewModel()
    {
        InitializeMenuItems();
    }

    private void InitializeMenuItems()
    {
        PrimaryMenuItems.Clear();

        PrimaryMenuItems.Add(new HomeMenuItem(
            "stock",
            "Stok",
            "lucide_package.svg",
            Color.FromArgb("#3B82F6"),
            _defaultRoutes["stock"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "case-entry",
            "Vaka Girişi",
            "lucide_file_text.svg",
            Color.FromArgb("#22C55E"),
            _defaultRoutes["case-entry"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "checklist",
            "Kontrol Listesi",
            "lucide_clipboard_check.svg",
            Color.FromArgb("#A855F7"),
            _defaultRoutes["checklist"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "history",
            "Geçmiş",
            "lucide_history.svg",
            Color.FromArgb("#F97316"),
            _defaultRoutes["history"]));

        PrimaryMenuItems.Add(new HomeMenuItem(
            "settings",
            "Ayarlar",
            "lucide_settings.svg",
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

        if (!string.IsNullOrWhiteSpace(item.Route))
        {
            await Shell.Current.GoToAsync(item.Route);
            return;
        }

        await Shell.Current.DisplayAlert("Bilgi", $"{item.Title} sayfası yakında eklenecek.", "Tamam");
    }
}
