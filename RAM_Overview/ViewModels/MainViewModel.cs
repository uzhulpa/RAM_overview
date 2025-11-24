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
            CurrentViewModel = new PerformanceViewModel(_memoryHardwareService, _memoryInfoService);
        }

        [RelayCommand]
        private void ShowProcessMonitor()
        {
            CurrentViewModel = new ProcessMonitorViewModel(_processMonitorService);
        }

        [RelayCommand]
        private void ShowSettings()
        {
            CurrentViewModel = new SettingsViewModel();
        }
    }
}
