using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Stok73.Models;

namespace Stok73.ViewModels;

public partial class CasePageViewModel : ObservableObject
{
    public ObservableCollection<CaseMaterialEntry> Materials { get; } = new();

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private string hospitalName = string.Empty;

    [ObservableProperty]
    private string doctorName = string.Empty;

    [ObservableProperty]
    private string patientName = string.Empty;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public CasePageViewModel()
    {
    }

    [RelayCommand]
    private void AddMaterial()
    {
        Materials.Add(new CaseMaterialEntry());
    }

    [RelayCommand]
    private void RemoveMaterial(CaseMaterialEntry? entry)
    {
        if (entry is null)
        {
            return;
        }

        Materials.Remove(entry);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (Shell.Current is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(HospitalName) || string.IsNullOrWhiteSpace(DoctorName) || string.IsNullOrWhiteSpace(PatientName))
        {
            await Shell.Current.DisplayAlert("Eksik bilgi", "Tüm zorunlu alanları doldurun.", "Tamam");
            return;
        }

        IsBusy = true;

        try
        {
            await Task.Delay(400); // TODO: persist case
            await Shell.Current.DisplayAlert("Kaydedildi", "Vaka kaydedildi.", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
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
}
