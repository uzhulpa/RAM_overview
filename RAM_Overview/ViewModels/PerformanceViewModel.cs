using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using RAM_Overview.Services;
using System.Collections.ObjectModel;

namespace RAM_Overview.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        private readonly MemoryHardwareService _memoryHardwareService;
        private readonly MemoryAllocationInfoService _memoryInfoService;
        public MemoryHardwareInfo MemoryHardwareInfo => _memoryHardwareService.MemoryInfo;
        [ObservableProperty] private MemoryAllocationInfo _memoryAllocationInfo;

        public PerformanceViewModel(MemoryHardwareService memoryHardwareService, MemoryAllocationInfoService memoryInfoService)
        {
            _memoryHardwareService = memoryHardwareService;
            _memoryInfoService = memoryInfoService;

            _memoryAllocationInfo = _memoryInfoService.MemoryAllocationInfo;

            // подписка на изменения чтобы обновлять 
            _memoryInfoService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MemoryAllocationInfoService.MemoryAllocationInfo))
                {
                    MemoryAllocationInfo = _memoryInfoService.MemoryAllocationInfo;
                }
            };
        }
    }
}
