using CommunityToolkit.Mvvm.ComponentModel;

namespace RAM_Overview.Models
{
    public partial class MemoryHardwareInfo : ObservableObject
    {
        [ObservableProperty] private string _memoryType = "MEMORY TYPE";

        [ObservableProperty] private int _speed;

        [ObservableProperty] private string _timings = "TIMINGS";

        [ObservableProperty] private int _slotsUsed;

        [ObservableProperty] private int _slotsTotal;

        [ObservableProperty] private string _formFactor = "FORM FACTOR";

        [ObservableProperty] private long _hardwareReserved;

        [ObservableProperty] private string _manufacturer = "MANUFACTURER";

        [ObservableProperty] private long _totalPhysicalMemory;

        public string SlotsInfo => $"{SlotsUsed} из {SlotsTotal}";
        public double HardwareReservedMB => HardwareReserved / 1024.0 / 1024.0;
        public double TotalPhysicalMemoryGB => TotalPhysicalMemory / 1024.0 / 1024.0 / 1024.0;

        partial void OnSlotsUsedChanged(int value) => OnPropertyChanged(nameof(SlotsInfo));
        partial void OnSlotsTotalChanged(int value) => OnPropertyChanged(nameof(SlotsInfo));
        partial void OnHardwareReservedChanged(long value) => OnPropertyChanged(nameof(HardwareReservedMB));
        partial void OnTotalPhysicalMemoryChanged(long value) => OnPropertyChanged(nameof(TotalPhysicalMemoryGB));
    }
}
