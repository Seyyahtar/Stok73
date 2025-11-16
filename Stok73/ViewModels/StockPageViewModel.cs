using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Stok73.Models;

namespace Stok73.ViewModels;

public partial class StockPageViewModel : ObservableObject
{
    private readonly List<StockItem> _allItems = new();
    private bool _isInitialized;

    public ObservableCollection<FilterOption> Filters { get; } = new();

    public ObservableCollection<StockGroup> VisibleGroups { get; } = new();

    public ObservableCollection<DeviceCategorySummary> DeviceSummaries { get; } = new();

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

    [ObservableProperty]
    private bool isFilterPanelVisible;

    [ObservableProperty]
    private bool isMenuVisible;

    [ObservableProperty]
    private Color filterButtonBackground = Colors.White;

    [ObservableProperty]
    private Color filterButtonTextColor = Color.FromArgb("#0F172A");

    public StockPageViewModel()
    {
        LoadFilters();
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        var loadedFromExcel = await LoadStockFromExcelAsync(showAlerts: false);

        if (!loadedFromExcel)
        {
            LoadSampleStock();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleFilterPanel()
    {
        IsFilterPanelVisible = !IsFilterPanelVisible;
    }

    [RelayCommand]
    private void ToggleMenu()
    {
        IsMenuVisible = !IsMenuVisible;
    }

    [RelayCommand]
    private async Task NavigateHomeAsync()
    {
        if (Shell.Current is null)
        {
            return;
        }

        await Shell.Current.GoToAsync("..");
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

    [RelayCommand]
    private void ToggleGroup(StockGroup? group)
    {
        if (group is null)
        {
            return;
        }

        group.IsExpanded = !group.IsExpanded;
    }

    [RelayCommand]
    private void ToggleMaterial(StockMaterialGroup? material)
    {
        if (material is null)
        {
            return;
        }

        material.IsExpanded = !material.IsExpanded;
    }

    [RelayCommand]
    private async Task OpenStockManagementAsync()
    {
        IsMenuVisible = false;

        if (Shell.Current is null)
        {
            return;
        }

        await Shell.Current.DisplayAlert("Bilgi", "Stok yönetimi ekranı henüz hazır değil.", "Tamam");
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        IsMenuVisible = false;

        try
        {
            if (_allItems.Count == 0)
            {
                await Shell.Current?.DisplayAlert("Bilgi", "Dışa aktarılacak stok bulunamadı.", "Tamam");
                return;
            }

            var exportPath = Path.Combine(FileSystem.CacheDirectory, $"stok-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Stok");
            WriteHeaders(worksheet);

            var rowIndex = 2;
            foreach (var item in _allItems)
            {
                WriteItemRow(worksheet, rowIndex, item);
                rowIndex++;
            }

            workbook.SaveAs(exportPath);

            await Shell.Current?.DisplayAlert("Bilgi", $"Excel dosyası {exportPath} konumuna kaydedildi.", "Tamam");
        }
        catch (Exception ex)
        {
            await Shell.Current?.DisplayAlert("Hata", $"Excel'e aktarılamadı: {ex.Message}", "Tamam");
        }
    }

    [RelayCommand]
    private async Task ImportFromExcelAsync()
    {
        IsMenuVisible = false;

        await LoadStockFromExcelAsync(showAlerts: true);
    }

    private async Task<bool> LoadStockFromExcelAsync(bool showAlerts)
    {
        try
        {
            await using var stream = await TryOpenExcelStreamAsync();

            if (stream is null)
            {
                if (showAlerts && Shell.Current is not null)
                {
                    await Shell.Current.DisplayAlert("Bilgi", "stok.xlsx dosyası bulunamadı. Lütfen dosyayı Resimler klasörüne ekleyin.", "Tamam");
                }

                return false;
            }

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null)
            {
                if (showAlerts && Shell.Current is not null)
                {
                    await Shell.Current.DisplayAlert("Hata", "Excel sayfası okunamadı.", "Tamam");
                }

                return false;
            }

            var importedItems = ParseWorksheet(worksheet);

            if (importedItems.Count == 0)
            {
                if (showAlerts && Shell.Current is not null)
                {
                    await Shell.Current.DisplayAlert("Bilgi", "Excel dosyasında aktarılabilir satır bulunamadı.", "Tamam");
                }

                return false;
            }

            ReplaceItems(importedItems);

            if (showAlerts && Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert("Bilgi", $"{importedItems.Count} satır stok listesine aktarıldı.", "Tamam");
            }

            return true;
        }
        catch (InvalidDataException ex)
        {
            if (showAlerts && Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert("Hata", ex.Message, "Tamam");
            }

            return false;
        }
        catch (Exception ex)
        {
            if (showAlerts && Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert("Hata", $"Excel içe aktarma hatası: {ex.Message}", "Tamam");
            }

            return false;
        }
    }

    [RelayCommand]
    private async Task OpenHistoryAsync()
    {
        IsMenuVisible = false;

        if (Shell.Current is null)
        {
            return;
        }

        await Shell.Current.DisplayAlert("Bilgi", "Geçmiş ekranı yakında eklenecek.", "Tamam");
    }

    [RelayCommand]
    private async Task RemoveItemAsync(StockItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Shell.Current?.DisplayAlert("Bilgi", $"{item.MaterialName} stoktan çıkarılacak modülü yakında eklenecek.", "Tamam");
    }

    [RelayCommand]
    private async Task DeleteItemAsync(StockItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Shell.Current?.DisplayAlert("Bilgi", $"{item.MaterialName} kaydı silme özelliği yakında eklenecek.", "Tamam");
    }

    [RelayCommand]
    private async Task EditItemAsync(StockItem? item)
    {
        if (item is null)
        {
            return;
        }

        await Shell.Current?.DisplayAlert("Bilgi", $"{item.MaterialName} düzenleme ekranı üzerinde çalışıyoruz.", "Tamam");
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
        var fallbackItems = new[]
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
        };

        ReplaceItems(fallbackItems);
    }

    private void ApplyFilters()
    {
        var activeKeys = Filters.Where(f => f.IsActive).Select(f => f.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filtered = _allItems.Where(item => MatchesSearch(item, SearchText) && MatchesFilters(item, activeKeys)).ToList();

        VisibleGroups.Clear();

        var materialGroups = filtered
            .GroupBy(item => item.MaterialName)
            .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var prefixGroups = materialGroups
            .GroupBy(group => group.Key.Split(' ')[0], StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.CurrentCultureIgnoreCase);

        foreach (var prefixGroup in prefixGroups)
        {
            var materials = prefixGroup
                .Select(materialGroup =>
                {
                    var orderedItems = materialGroup
                        .OrderBy(item => item.ExpiryDate)
                        .ThenBy(item => item.SerialLotNumber)
                        .ToList();

                    var firstItem = orderedItems.First();
                    var subtitle = $"{firstItem.CategoryName} • {firstItem.Manufacturer}";

                    return new StockMaterialGroup(materialGroup.Key, subtitle, orderedItems);
                })
                .ToList();

            VisibleGroups.Add(new StockGroup(prefixGroup.Key.ToUpperInvariant(), materials));
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

        UpdateDeviceSummaries(filtered);
        UpdateFilterButtonVisual();
    }

    private void ReplaceItems(IEnumerable<StockItem> items)
    {
        _allItems.Clear();
        _allItems.AddRange(items);
        ApplyFilters();
    }

    private static List<StockItem> ParseWorksheet(IXLWorksheet worksheet)
    {
        var headers = worksheet
            .Row(1)
            .Cells()
            .Where(cell => !string.IsNullOrWhiteSpace(cell.GetString()))
            .ToDictionary(cell => cell.GetString().Trim(), cell => cell.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

        if (!headers.ContainsKey("materialname"))
        {
            throw new InvalidDataException("Excel dosyasında MaterialName sütunu bulunamadı.");
        }

        var importedItems = new List<StockItem>();

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var materialName = GetCellValue(row, headers, "materialname");

            if (string.IsNullOrWhiteSpace(materialName))
            {
                continue;
            }

            var serial = GetCellValue(row, headers, "seriallotnumber");
            var ubb = GetCellValue(row, headers, "ubbcode");
            var expiry = GetDateValue(row, headers, "expirydate");
            var quantity = GetIntValue(row, headers, "quantity");
            var location = GetCellValue(row, headers, "location", "Depo");
            var categoryKey = GetCellValue(row, headers, "categorykey", "lead");
            var categoryName = GetCellValue(row, headers, "categoryname", "Kategori");
            var manufacturer = GetCellValue(row, headers, "manufacturer", "Üretici");

            importedItems.Add(new StockItem(Guid.NewGuid().ToString(), materialName, serial, ubb, expiry, quantity, location, categoryKey, categoryName, manufacturer));
        }

        return importedItems;
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

    private void UpdateDeviceSummaries(IReadOnlyCollection<StockItem> items)
    {
        DeviceSummaries.Clear();

        var pacemakerTotal = items.Where(IsPacemaker).Sum(item => item.Quantity);
        var icdTotal = items.Where(IsIcd).Sum(item => item.Quantity);
        var crtTotal = items.Where(IsCrt).Sum(item => item.Quantity);

        DeviceSummaries.Add(new DeviceCategorySummary("Pacemaker", "Amvia Sky, Endicos, Enitra, Edora", pacemakerTotal, Color.FromArgb("#DBEAFE"), Color.FromArgb("#1D4ED8")));
        DeviceSummaries.Add(new DeviceCategorySummary("ICD", "VR-T ve DR-T cihazları", icdTotal, Color.FromArgb("#DCFCE7"), Color.FromArgb("#15803D")));
        DeviceSummaries.Add(new DeviceCategorySummary("CRT", "HF-T cihazları", crtTotal, Color.FromArgb("#F3E8FF"), Color.FromArgb("#7C3AED")));
    }

    private static bool IsPacemaker(StockItem item)
    {
        var name = item.MaterialName.ToLowerInvariant();
        return name.Contains("amvia sky") || name.Contains("endicos") || name.Contains("enitra") || name.Contains("edora");
    }

    private static bool IsIcd(StockItem item)
    {
        var name = item.MaterialName.ToLowerInvariant();

        if (IsPacemaker(item))
        {
            return false;
        }

        return name.Contains("vr-t") || name.Contains("dr-t");
    }

    private static bool IsCrt(StockItem item)
    {
        var name = item.MaterialName.ToLowerInvariant();

        if (IsPacemaker(item) || IsIcd(item))
        {
            return false;
        }

        return name.Contains("hf-t");
    }

    partial void OnHasActiveFiltersChanged(bool value) => UpdateFilterButtonVisual();

    partial void OnIsFilterPanelVisibleChanged(bool value) => UpdateFilterButtonVisual();

    private void UpdateFilterButtonVisual()
    {
        var highlight = HasActiveFilters || IsFilterPanelVisible;
        FilterButtonBackground = highlight ? Color.FromArgb("#DBEAFE") : Colors.White;
        FilterButtonTextColor = highlight ? Color.FromArgb("#1D4ED8") : Color.FromArgb("#0F172A");
    }

    private static void WriteHeaders(IXLWorksheet worksheet)
    {
        var headers = new[]
        {
            "MaterialName",
            "SerialLotNumber",
            "UbbCode",
            "ExpiryDate",
            "Quantity",
            "Location",
            "CategoryKey",
            "CategoryName",
            "Manufacturer"
        };

        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
            worksheet.Cell(1, column + 1).Style.Font.SetBold();
        }
    }

    private static void WriteItemRow(IXLWorksheet worksheet, int rowIndex, StockItem item)
    {
        worksheet.Cell(rowIndex, 1).Value = item.MaterialName;
        worksheet.Cell(rowIndex, 2).Value = item.SerialLotNumber;
        worksheet.Cell(rowIndex, 3).Value = item.UbbCode;
        worksheet.Cell(rowIndex, 4).Value = item.ExpiryDate;
        worksheet.Cell(rowIndex, 5).Value = item.Quantity;
        worksheet.Cell(rowIndex, 6).Value = item.Location;
        worksheet.Cell(rowIndex, 7).Value = item.CategoryKey;
        worksheet.Cell(rowIndex, 8).Value = item.CategoryName;
        worksheet.Cell(rowIndex, 9).Value = item.Manufacturer;
    }

    private static string GetCellValue(IXLRow row, IReadOnlyDictionary<string, int> headers, string key, string fallback = "")
    {
        return headers.TryGetValue(key, out var column)
            ? row.Cell(column).GetString()
            : fallback;
    }

    private static DateTime GetDateValue(IXLRow row, IReadOnlyDictionary<string, int> headers, string key)
    {
        if (headers.TryGetValue(key, out var column))
        {
            var cell = row.Cell(column);
            if (cell.TryGetValue(out DateTime date))
            {
                return date;
            }

            if (DateTime.TryParse(cell.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return DateTime.Today;
    }

    private static int GetIntValue(IXLRow row, IReadOnlyDictionary<string, int> headers, string key)
    {
        if (headers.TryGetValue(key, out var column))
        {
            var cell = row.Cell(column);
            if (cell.TryGetValue(out int value))
            {
                return value;
            }

            if (int.TryParse(cell.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static async Task<Stream?> TryOpenExcelStreamAsync()
    {
        var candidates = new List<string>
        {
            Path.Combine(FileSystem.AppDataDirectory, "Resimler", "stok.xlsx"),
            Path.Combine(FileSystem.AppDataDirectory, "stok.xlsx")
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                return File.OpenRead(path);
            }
        }

        try
        {
            return await FileSystem.OpenAppPackageFileAsync(Path.Combine("Resimler", "stok.xlsx"));
        }
        catch
        {
            // ignored
        }

        try
        {
            return await FileSystem.OpenAppPackageFileAsync("stok.xlsx");
        }
        catch
        {
            return null;
        }
    }
}
