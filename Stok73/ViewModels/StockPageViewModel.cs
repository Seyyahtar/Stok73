using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Stok73.Models;

namespace Stok73.ViewModels;

public partial class StockPageViewModel : ObservableObject
{
    private readonly List<StockItem> _allItems = new();

    public ObservableCollection<FilterOption> Filters { get; } = new();

    public ObservableCollection<StockGroup> VisibleGroups { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private int totalQuantity;

    [ObservableProperty]
    private int distinctMaterialCount;

    [ObservableProperty]
    private string nextExpiryDisplay = "Tanımlı değil";

    [ObservableProperty]
    private bool hasResults;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private bool hasActiveFilters;

    public StockPageViewModel()
    {
        LoadFilters();
        LoadSampleStock();
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleFilter(FilterOption? option)
    {
        if (option is null)
        {
            return;
        }

        option.IsActive = !option.IsActive;
        ApplyFilters();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            SearchText = string.Empty;
        }

        foreach (var filter in Filters)
        {
            filter.IsActive = false;
        }

        ApplyFilters();
    }

    private void LoadFilters()
    {
        Filters.Clear();
        Filters.Add(new FilterOption("lead", "Lead", Color.FromArgb("#3B82F6")));
        Filters.Add(new FilterOption("sheath", "Sheath", Color.FromArgb("#0EA5E9")));
        Filters.Add(new FilterOption("pacemaker", "Pacemaker", Color.FromArgb("#10B981")));
        Filters.Add(new FilterOption("icd", "ICD", Color.FromArgb("#F97316")));
        Filters.Add(new FilterOption("crt", "CRT", Color.FromArgb("#8B5CF6")));
    }

    private void LoadSampleStock()
    {
        _allItems.Clear();

        _allItems.AddRange(new[]
        {
            new StockItem("1", "Solia S53", "SN-10234", "1234567890123", DateTime.Today.AddMonths(8), 6, "Depo A", "lead", "Kalp Pili Lead", "Biotronik"),
            new StockItem("2", "Solia S60", "SN-10256", "1234567890456", DateTime.Today.AddMonths(4), 4, "Depo A", "lead", "Kalp Pili Lead", "Biotronik"),
            new StockItem("3", "Safesheath CSG 8.5F", "SN-20894", "1234567890789", DateTime.Today.AddMonths(2), 9, "Depo B", "sheath", "Sheath", "Merit"),
            new StockItem("4", "Safesheath Mini 6F", "SN-20941", "1234567890876", DateTime.Today.AddMonths(10), 12, "Depo B", "sheath", "Sheath", "Merit"),
            new StockItem("5", "Endicos SR-T", "SN-30872", "9876543210123", DateTime.Today.AddMonths(6), 3, "Depo C", "pacemaker", "Kalp Pili", "Biotronik"),
            new StockItem("6", "Amvia Sky DR-T", "SN-30934", "9876543210456", DateTime.Today.AddMonths(3), 2, "Depo C", "icd", "ICD", "Biotronik"),
            new StockItem("7", "Plexa ProMRI S53", "SN-40231", "9876543210789", DateTime.Today.AddMonths(12), 5, "Depo A", "lead", "Kalp Pili Lead", "Biotronik"),
            new StockItem("8", "Sentus OTW QP L", "SN-50912", "9876543210876", DateTime.Today.AddMonths(1), 2, "Depo D", "crt", "CRT Lead", "Biotronik"),
            new StockItem("9", "Li-7 Sheath", "SN-60256", "4567891230123", DateTime.Today.AddMonths(5), 7, "Depo B", "sheath", "Sheath", "Lifeline"),
            new StockItem("10", "Enitra 8 DR-T", "SN-70982", "4567891230456", DateTime.Today.AddMonths(9), 4, "Depo C", "icd", "ICD", "Biotronik"),
        });
    }

    private void ApplyFilters()
    {
        var activeKeys = Filters.Where(f => f.IsActive).Select(f => f.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filtered = _allItems.Where(item => MatchesSearch(item, SearchText) && MatchesFilters(item, activeKeys)).ToList();

        VisibleGroups.Clear();

        var grouped = filtered
            .GroupBy(item => item.CategoryName)
            .OrderBy(group => group.Key);

        foreach (var group in grouped)
        {
            var orderedItems = group
                .OrderBy(item => item.ExpiryDate)
                .ThenBy(item => item.MaterialName)
                .ToList();

            VisibleGroups.Add(new StockGroup(group.Key, orderedItems.Sum(item => item.Quantity), orderedItems));
        }

        TotalQuantity = filtered.Sum(item => item.Quantity);
        DistinctMaterialCount = filtered.Count;
        HasResults = filtered.Count > 0;
        IsEmpty = filtered.Count == 0;
        HasActiveFilters = activeKeys.Count > 0 || !string.IsNullOrWhiteSpace(SearchText);

        if (filtered.Count > 0)
        {
            var earliestExpiry = filtered.Min(item => item.ExpiryDate);
            NextExpiryDisplay = earliestExpiry.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        }
        else
        {
            NextExpiryDisplay = "Tanımlı değil";
        }
    }

    private static bool MatchesSearch(StockItem item, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var query = search.Trim().ToLowerInvariant();

        return item.MaterialName.ToLowerInvariant().Contains(query)
            || item.SerialLotNumber.ToLowerInvariant().Contains(query)
            || item.UbbCode.ToLowerInvariant().Contains(query)
            || item.Location.ToLowerInvariant().Contains(query);
    }

    private static bool MatchesFilters(StockItem item, HashSet<string> activeKeys)
    {
        if (activeKeys.Count == 0)
        {
            return true;
        }

        return activeKeys.Contains(item.CategoryKey);
    }
}
