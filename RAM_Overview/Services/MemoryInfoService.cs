using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;

namespace RAM_Overview.Services
{
    public partial class MemoryAllocationInfoService : ObservableObject
    {
        [ObservableProperty] private MemoryAllocationInfo _memoryAllocationInfo = new();

        private readonly Timer _timer;
        private readonly PerformanceCounter _availableBytesCounter;
        private readonly PerformanceCounter _committedBytesCounter;
        private readonly PerformanceCounter _pagedPoolBytesCounter;
        private readonly PerformanceCounter _nonPagedPoolBytesCounter;

        // для кэшированной памяти
        private readonly PerformanceCounter _cacheBytesCounter1;
        private readonly PerformanceCounter _cacheBytesCounter2;
        private readonly PerformanceCounter _cacheBytesCounter3;
        private readonly PerformanceCounter _cacheBytesCounter4;
        private readonly PerformanceCounter _cacheBytesCounter5;

        public MemoryAllocationInfoService()
        {
            // Инициализация Performance Counters
            _availableBytesCounter = new PerformanceCounter("Memory", "Available Bytes");
            _committedBytesCounter = new PerformanceCounter("Memory", "Committed Bytes");
            _pagedPoolBytesCounter = new PerformanceCounter("Memory", "Pool Paged Bytes");
            _nonPagedPoolBytesCounter = new PerformanceCounter("Memory", "Pool Nonpaged Bytes");

            // кэшированная память
            _cacheBytesCounter1 = new PerformanceCounter("Memory", "Cache Bytes");
            _cacheBytesCounter2 = new PerformanceCounter("Memory", "Modified Page List Bytes");
            _cacheBytesCounter3 = new PerformanceCounter("Memory", "Standby Cache Reserve Bytes");
            _cacheBytesCounter4 = new PerformanceCounter("Memory", "Standby Cache Normal Priority Bytes");
            _cacheBytesCounter5 = new PerformanceCounter("Memory", "Standby Cache Core Bytes");

            // Запуск таймера обновления (каждые 2 секунды)
            _timer = new Timer(UpdateMemoryInfo, null, TimeSpan.Zero, TimeSpan.FromSeconds(2.5));

            // Первоначальная загрузка данных
            UpdateMemoryInfo(null);
        }

        private void UpdateMemoryInfo(object? state)
        {
            try
            {
                var newMemoryInfo = new MemoryAllocationInfo
                {
                    LastUpdateTime = DateTime.Now
                };

                // Получение базовой информации через GlobalMemoryStatusEx
                GetBasicMemoryInfo(newMemoryInfo);

                // Получение дополнительной информации через Performance Counters
                GetPerformanceCounterInfo(newMemoryInfo);

                // Получение информации через WMI
                GetWmiMemoryInfo(newMemoryInfo);

                // Расчет производных значений
                CalculateDerivedValues(newMemoryInfo);

                // Обновление в UI потоке
                Application.Current?.Dispatcher.Invoke((Action)(() =>
                {
                    MemoryAllocationInfo = newMemoryInfo;
                }));

                Debug.WriteLine($"UsedMemoryBytes = {MemoryAllocationInfo.UsedMemoryBytes} ({MemoryAllocationInfo.LastUpdateTime.ToString()})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка обновления информации о памяти: {ex.Message}");
            }
        }

        private void GetBasicMemoryInfo(MemoryAllocationInfo memoryInfo)
        {
            // Используем GlobalMemoryStatusEx через P/Invoke или WMI
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                memoryInfo.TotalPhysicalMemoryBytes = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                memoryInfo.FreeMemoryBytes = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                memoryInfo.CommittedMemoryBytes = Convert.ToUInt64(obj["TotalVirtualMemorySize"]) * 1024 -
                                                Convert.ToUInt64(obj["FreeVirtualMemory"]) * 1024;
                memoryInfo.CommitLimitBytes = Convert.ToUInt64(obj["TotalVirtualMemorySize"]) * 1024;

                break;
            }
        }

        private void GetPerformanceCounterInfo(MemoryAllocationInfo memoryInfo)
        {
            memoryInfo.AvailableMemoryBytes = (ulong)_availableBytesCounter.NextValue();
            memoryInfo.PagedPoolBytes = (ulong)_pagedPoolBytesCounter.NextValue();
            memoryInfo.NonPagedPoolBytes = (ulong)_nonPagedPoolBytesCounter.NextValue();

            memoryInfo.CachedMemoryBytes = (ulong)(_cacheBytesCounter1.NextValue()
                                                + _cacheBytesCounter2.NextValue()
                                                + _cacheBytesCounter3.NextValue()
                                                + _cacheBytesCounter4.NextValue()
                                                + _cacheBytesCounter5.NextValue());
        }

        private void GetWmiMemoryInfo(MemoryAllocationInfo memoryInfo)
        {
            try
            {
                // Получение информации о сжатой памяти (Windows 10+)
                using var memSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in memSearcher.Get())
                {
                    // Для сжатой памяти может потребоваться дополнительная логика
                    // В некоторых системах это доступно через другие счетчики
                    memoryInfo.CompressedMemoryBytes = CalculateCompressedMemory();
                    break;
                }
            }
            catch
            {
                memoryInfo.CompressedMemoryBytes = 0;
            }
        }

        private void CalculateDerivedValues(MemoryAllocationInfo memoryInfo)
        {
            // Используемая память = Общая - Доступная
            memoryInfo.UsedMemoryBytes = memoryInfo.TotalPhysicalMemoryBytes - memoryInfo.AvailableMemoryBytes;

            // Если сжатая память не определена, используем приблизительное значение
            if (memoryInfo.CompressedMemoryBytes == 0)
            {
                memoryInfo.CompressedMemoryBytes = (ulong)(memoryInfo.UsedMemoryBytes * 0.05); // ~5%
            }
        }

        private ulong CalculateCompressedMemory()
        {
            // Сложная логика определения сжатой памяти
            // Может потребовать P/Invoke вызовов или использования дополнительных счетчиков
            try
            {
                // Временная реализация - можно улучшить
                using var counter = new PerformanceCounter("Memory", "Pool Paged Resident Bytes");
                return (ulong)counter.NextValue();
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _availableBytesCounter?.Dispose();
            _committedBytesCounter?.Dispose();
            _pagedPoolBytesCounter?.Dispose();
            _nonPagedPoolBytesCounter?.Dispose();

            _cacheBytesCounter1?.Dispose();
            _cacheBytesCounter2?.Dispose();
            _cacheBytesCounter3?.Dispose();
            _cacheBytesCounter4?.Dispose();
            _cacheBytesCounter5?.Dispose();
        }
    }
}