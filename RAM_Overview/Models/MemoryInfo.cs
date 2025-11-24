using CommunityToolkit.Mvvm.ComponentModel;

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
        public double UsedMemoryGB => BytesToGB(UsedMemoryBytes);
        public double CompressedMemoryMB => BytesToMB(CompressedMemoryBytes);
        public double CommittedMemoryGB => BytesToGB(CommittedMemoryBytes);
        public double CommitLimitGB => BytesToGB(CommitLimitBytes);
        public double AvailableMemoryGB => BytesToGB(AvailableMemoryBytes);
        public double CachedMemoryGB => BytesToGB(CachedMemoryBytes);
        public double PagedPoolMB => BytesToMB(PagedPoolBytes);
        public double NonPagedPoolMB => BytesToMB(NonPagedPoolBytes);

        private static double BytesToGB(ulong bytes) => Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 1, MidpointRounding.AwayFromZero);
        private static double BytesToMB(ulong bytes) => Math.Round(bytes / 1024.0 / 1024.0);
    }
}
