using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Services.Converters;

namespace RAM_Overview.Models
{
    public partial class MemoryAllocationInfo : ObservableObject
    {
        // используемая память + сжатая
        [ObservableProperty] private ulong _usedMemoryBytes;
        [ObservableProperty] private ulong _compressedMemoryBytes;

        // выделенная память + ее лимит
        [ObservableProperty] private ulong _committedMemoryBytes;
        [ObservableProperty] private ulong _commitLimitBytes;

        // доступная память
        [ObservableProperty] private ulong _availableMemoryBytes;

        // кэшированная память
        // Cache Bytes + Modified Page List Bytes + Standby Cache Reserve Bytes + Standby Cache Normal Priority Bytes + Standby Cache Code Bytes
        [ObservableProperty] private ulong _cachedMemoryBytes;

        // выгружаемый пул
        [ObservableProperty] private ulong _pagedPoolBytes;

        // невыгружаемый пул
        [ObservableProperty] private ulong _nonPagedPoolBytes;

        // доп
        [ObservableProperty] private ulong _totalPhysicalMemoryBytes;
        [ObservableProperty] private ulong _freeMemoryBytes;
        [ObservableProperty] private DateTime _lastUpdateTime;

        // вычисляемые свойства
        public string UsedMemoryString => MemoryUnitConverter.BytesToAutoString(UsedMemoryBytes);
        public double UsedMemoryGB => BytesToGB(UsedMemoryBytes);

        public string CompressedMemoryString => MemoryUnitConverter.BytesToAutoString(CompressedMemoryBytes);
        public string CommittedMemoryString => MemoryUnitConverter.BytesToAutoString(CommittedMemoryBytes);
        public string CommitLimitString => MemoryUnitConverter.BytesToAutoString(CommitLimitBytes);
        public string AvailableMemoryString => MemoryUnitConverter.BytesToAutoString(AvailableMemoryBytes);
        public string CachedMemoryString => MemoryUnitConverter.BytesToAutoString(CachedMemoryBytes);
        public string PagedPoolString => MemoryUnitConverter.BytesToAutoString(PagedPoolBytes);
        public string NonPagedPoolString => MemoryUnitConverter.BytesToAutoString(NonPagedPoolBytes);

        private static double BytesToGB(ulong bytes) => Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 1, MidpointRounding.AwayFromZero);
        private static double BytesToMB(ulong bytes) => Math.Round(bytes / 1024.0 / 1024.0);
    }
}
