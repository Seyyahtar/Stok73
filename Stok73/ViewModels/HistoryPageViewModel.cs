using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Stok73.Models;

namespace Stok73.ViewModels;

public partial class HistoryPageViewModel : ObservableObject
{
    public ObservableCollection<HistoryItem> Items { get; } = new();

    [ObservableProperty]
    private string selectedType = "Tümü";

    [ObservableProperty]
    private DateTime? startDate;

    [ObservableProperty]
    private DateTime? endDate;

    public HistoryPageViewModel()
    {
        LoadSample();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedType = "Tümü";
        StartDate = null;
        EndDate = null;
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        if (Shell.Current is null)
        {
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private void LoadSample()
    {
        Items.Clear();
        Items.Add(new HistoryItem(
            "Vaka",
            "Vaka kaydı - Fj - Dr. Fhk",
            string.Empty,
            DateTime.Today.AddDays(-1),
            Color.FromArgb("#DBEAFE"),
            Color.FromArgb("#1D4ED8")));

        Items.Add(new HistoryItem(
            "Kontrol Listesi",
            "Kontrol listesi tamamlandı - 5/35 hasta kontrol edildi",
            string.Empty,
            DateTime.Today.AddDays(-1),
            Color.FromArgb("#F4E9FF"),
            Color.FromArgb("#9333EA")));

        Items.Add(new HistoryItem(
            "Stok Silme",
            "Adelante 7 F silindi (2 adet) - İbrahim",
            string.Empty,
            DateTime.Today.AddDays(-1),
            Color.FromArgb("#FFEFD6"),
            Color.FromArgb("#EA580C")));
    }
}
