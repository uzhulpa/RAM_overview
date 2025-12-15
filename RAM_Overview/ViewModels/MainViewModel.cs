using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RAM_Overview.Services;
using RAM_Overview.Views;

namespace RAM_Overview.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ProcessMonitorService _processMonitorService;
        private readonly MemoryHardwareService _memoryHardwareService;
        private readonly MemoryAllocationInfoService _memoryInfoService;

        [ObservableProperty] private object? _currentViewModel;

        private PerformanceViewModel? _performanceViewModel;
        private ProcessMonitorViewModel? _processMonitorViewModel;

        [ObservableProperty] private bool _isPerformanceActive;
        [ObservableProperty] private bool _isProcessMonitorActive;
        [ObservableProperty] private bool _isSettingsActive;

        public MainViewModel(ProcessMonitorService processMonitorService, MemoryHardwareService memoryHardwareService, MemoryAllocationInfoService memoryInfoService)
        {
            _processMonitorService = processMonitorService;
            _memoryHardwareService = memoryHardwareService;
            _memoryInfoService = memoryInfoService;

            ShowPerformance();
        }

        [RelayCommand]
        private void ShowPerformance()
        {
            if (_performanceViewModel == null) _performanceViewModel = new PerformanceViewModel(_memoryHardwareService, _memoryInfoService, _processMonitorService);
            CurrentViewModel = _performanceViewModel;

            IsPerformanceActive = true;
            IsProcessMonitorActive = false;
            IsSettingsActive = false;
        }

        [RelayCommand]
        private void ShowProcessMonitor()
        {
            if (_processMonitorViewModel == null) _processMonitorViewModel = new ProcessMonitorViewModel(_processMonitorService);
            CurrentViewModel = _processMonitorViewModel;

            IsPerformanceActive = false;
            IsProcessMonitorActive = true;
            IsSettingsActive = false;
        }

        [RelayCommand]
        private void ShowSettings()
        {
            CurrentViewModel = new SettingsViewModel();

            IsPerformanceActive = false;
            IsProcessMonitorActive = false;
            IsSettingsActive = true;
        }
    }
}
