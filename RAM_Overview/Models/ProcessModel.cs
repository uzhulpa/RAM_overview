using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using RAM_Overview.Services.Converters;

namespace RAM_Overview.Models
{
    public partial class ProcessModel : ObservableObject
    {
        // для основной таблицы
        [ObservableProperty] private ImageSource _icon;
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = "PROCESS_NAME";
        // + воркинг сет
        [ObservableProperty] private TimeSpan _runningTime;

        [ObservableProperty] private Brush _chartColor = Brushes.Transparent;

        // распределение памяти
        [ObservableProperty] private long _workingSetBytes;
        [ObservableProperty] private long _privateMemoryBytes;
        [ObservableProperty] private long _peakWorkingSetBytes;
        [ObservableProperty] private long _virtualMemoryBytes;
        [ObservableProperty] private long _peakVirtualMemoryBytes;
        [ObservableProperty] private long _pagedMemoryBytes;
        [ObservableProperty] private long _nonpagedMemoryBytes;

        // свойства самого процесса
        [ObservableProperty] private string _processFileName = "PROCESS_FILE_NAME";
        [ObservableProperty] private DateTime _startTime;
        [ObservableProperty] private int _basePriority;
        [ObservableProperty] private int _threadsCount;
        [ObservableProperty] private int _handleCount;
        [ObservableProperty] private string _processOwner = "PROCESS_OWNER";
        [ObservableProperty] private string _certificateIssuer = "CERTIFICATE_ISSUER";

        public double WorkingSetMB => Math.Round(WorkingSetBytes / 1024.0 / 1024.0, 1);

        public string WorkingSetString => MemoryUnitConverter.BytesToAutoString(WorkingSetBytes);
        public string PrivateMemoryString => MemoryUnitConverter.BytesToAutoString(PrivateMemoryBytes);
        public string PeakWorkingSetString => MemoryUnitConverter.BytesToAutoString(PeakWorkingSetBytes);
        public string VirtualMemoryString => MemoryUnitConverter.BytesToAutoString(VirtualMemoryBytes);
        public string PeakVirtualMemoryString => MemoryUnitConverter.BytesToAutoString(PeakVirtualMemoryBytes);
        public string PagedMemoryString => MemoryUnitConverter.BytesToAutoString(PagedMemoryBytes);
        public string NonPagedMemoryString => MemoryUnitConverter.BytesToAutoString(NonpagedMemoryBytes);


        partial void OnWorkingSetBytesChanged(long value)
        {
            OnPropertyChanged(nameof(WorkingSetMB));
        }
    }
}
