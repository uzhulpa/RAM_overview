using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        // распределение памяти
        [ObservableProperty] private long _workingSetBytes;
        [ObservableProperty] private long _privateMemoryBytes;
        [ObservableProperty] private long _peakWorkingSetBytes;
        [ObservableProperty] private long _virtualMemoryBytes;
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
    }
}
