using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using RAM_Overview.Models;
using RAM_Overview.Services;
using RAM_Overview.Services.Converters;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

namespace RAM_Overview.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject
    {
        private readonly MemoryHardwareService _memoryHardwareService;
        private readonly MemoryAllocationInfoService _memoryInfoService;
        private readonly ProcessMonitorService _processMonitorService;

        public MemoryHardwareInfo MemoryHardwareInfo => _memoryHardwareService.MemoryInfo;

        [ObservableProperty]
        private MemoryAllocationInfo _memoryAllocationInfo;

        // ГРАФИК ИСПОЛЬЗОВАНИЯ ПАМЯТИ
        [ObservableProperty] private ISeries[] _memoryUsageSeries;
        [ObservableProperty] private Axis[] _xAxes;
        [ObservableProperty] private Axis[] _yAxes;
        private readonly Queue<double> _memoryUsageHistory;
        private readonly Queue<string> _timeLabels;
        private readonly List<DateTime> _chartTimestamps;
        private const int MAX_DATA_POINTS = 40;
        private const double DONUT_INNER_RADIUS = 0d;
        private const int PRIMARY_PROCESS_COLOR_COUNT = 5;

        // КРУГОВАЯ ДИАГРАММА С ТОП 5 ПРОЦЕССОВ
        [ObservableProperty] private ISeries[] _processMemorySeries;
        [ObservableProperty] private ObservableCollection<ProcessModel> _topProcesses;

        // цвета для процессов
        private static readonly SKColor[] ProcessColors = new[]
        {
            new SKColor(165, 53, 137, 130),     // розовый
            new SKColor(0, 140, 81, 130),     // зеленый
            new SKColor(251, 232, 1, 130),     // желтый
            new SKColor(228, 40, 44, 130),      // красный
            new SKColor(94, 44, 128, 130),     // фиолетовый
            new SKColor(0, 89, 169, 130),    // синий
            new SKColor(118, 118, 118, 130)          // серый
    };

        private static readonly SKColor[] ProcessStrokeColors = new[]
        {
            new SKColor(86, 1, 68, 180),         // темный розовый
            new SKColor(0, 69, 18, 180),         // темный зеленый
            new SKColor(146, 133, 2, 180),         // темный желтый
            new SKColor(137, 1, 5, 180),         // темный красный
            new SKColor(24, 3, 62, 180),        // темный фиолетовый
            new SKColor(1, 32, 97, 180),     // темный синий
            new SKColor(68, 68, 68, 180)      // темный серый
        };

        public PerformanceViewModel(MemoryHardwareService memoryHardwareService, MemoryAllocationInfoService memoryInfoService, ProcessMonitorService processMonitorService)
        {
            _memoryHardwareService = memoryHardwareService;
            _memoryInfoService = memoryInfoService;
            _processMonitorService = processMonitorService;

            _memoryAllocationInfo = _memoryInfoService.MemoryAllocationInfo;
            _memoryUsageHistory = new Queue<double>();
            _timeLabels = new Queue<string>();
            _chartTimestamps = new List<DateTime>();
            _topProcesses = new ObservableCollection<ProcessModel>();

            InitializeChart();
            InitializeProcessChart();

            _memoryInfoService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MemoryAllocationInfoService.MemoryAllocationInfo))
                {
                    MemoryAllocationInfo = _memoryInfoService.MemoryAllocationInfo;
                    UpdateChartData();
                }
            };

            _processMonitorService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProcessMonitorService.ProcessModels))
                {
                    UpdateProcessChartData();
                }
            };

            UpdateProcessChartData();
        }

        private void InitializeChart()
        {
            MemoryUsageSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Используется",
                    Values = new List<double>(),
                    Stroke = new SolidColorPaint(SKColors.Blue, 2),
                    Fill = new LinearGradientPaint(
                        new[] {
                            new SKColor(59, 130, 246, 150),
                            new SKColor(59, 130, 246, 50)
                        },
                        new SKPoint(0.5f, 1),
                        new SKPoint(0.5f, 0)
                    ),
                    GeometrySize = 0,
                    LineSmoothness = 0.6
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Время",
                    LabelsPaint = new SolidColorPaint(SKColors.Transparent),
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    NameTextSize = 0,
                    TextSize = 0,
                    LabelsRotation = 0
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
                    TextSize = 12,
                    Labeler = value => value.ToString("0.# ГБ")
                }
            };
        }

        private void UpdateChartData()
        {
            double usedMemoryGB = MemoryAllocationInfo.UsedMemoryGB;
            DateTime currentTime = DateTime.Now;

            _memoryUsageHistory.Enqueue(usedMemoryGB);
            _timeLabels.Enqueue(currentTime.ToString("HH:mm:ss"));
            _chartTimestamps.Add(currentTime);

            while (_memoryUsageHistory.Count > MAX_DATA_POINTS)
            {
                _memoryUsageHistory.Dequeue();
                _timeLabels.Dequeue();
            }

            while (_chartTimestamps.Count > MAX_DATA_POINTS)
            {
                _chartTimestamps.RemoveAt(0);
            }

            var series = MemoryUsageSeries[0] as LineSeries<double>;
            if (series != null)
            {
                var values = new List<double>();

                int emptyPoints = MAX_DATA_POINTS - _memoryUsageHistory.Count;
                for (int i = 0; i < emptyPoints; i++)
                {
                    values.Add(0);
                }

                values.AddRange(_memoryUsageHistory);
                series.Values = values;
            }

            double maxMemoryGB = MemoryHardwareInfo.TotalPhysicalMemoryGB -
                               (MemoryHardwareInfo.HardwareReservedBytes / 1024.0 / 1024.0 / 1024.0);

            var yAxis = YAxes.FirstOrDefault();
            if (yAxis != null)
            {
                yAxis.MaxLimit = Math.Max(maxMemoryGB, 0);
            }

            OnPropertyChanged(nameof(MemoryUsageSeries));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }

        private void InitializeProcessChart()
        {
            ProcessMemorySeries = Array.Empty<ISeries>();
        }

        private void UpdateProcessChartData()
        {
            if (MemoryAllocationInfo == null)
            {
                ProcessMemorySeries = Array.Empty<ISeries>();
                OnPropertyChanged(nameof(ProcessMemorySeries));
                return;
            }

            var allProcesses = _processMonitorService.ProcessModels
                .Where(p => p.WorkingSetBytes > 0)
                .OrderByDescending(p => p.WorkingSetBytes)
                .ToList();

            var topProcesses = allProcesses.Take(5).ToList();

            TopProcesses.Clear();
            foreach (var process in topProcesses)
            {
                TopProcesses.Add(process);
            }

            ulong topProcessesMemoryBytes = (ulong)topProcesses.Sum(p => p.WorkingSetBytes);
            ulong usedMemoryBytes = MemoryAllocationInfo.UsedMemoryBytes;

            ulong otherProcessesMemoryBytes = 0;
            if (usedMemoryBytes > topProcessesMemoryBytes)
            {
                otherProcessesMemoryBytes = usedMemoryBytes - topProcessesMemoryBytes;
            }

            var series = new List<ISeries>();

            //AddAvailableMemorySlice(series, MemoryAllocationInfo.AvailableMemoryBytes);

            for (int i = 0; i < topProcesses.Count; i++)
            {
                var process = topProcesses[i];
                double workingSetGb = MemoryUnitConverter.BytesToGB((ulong)process.WorkingSetBytes);
                if (workingSetGb <= 0) continue;

                int paletteIndex = Math.Min(i, PRIMARY_PROCESS_COLOR_COUNT - 1);
                var fillColor = ProcessColors[paletteIndex];
                var strokeColor = ProcessStrokeColors[paletteIndex];

                process.ChartColor = CreateBrush(fillColor);

                series.Add(new PieSeries<double>
                {
                    Values = new[] { workingSetGb },
                    Name = FormatProcessName(process.Name, process.Id),
                    Fill = new SolidColorPaint(ProcessColors[i]),
                    HoverPushout = 6,
                    InnerRadius = DONUT_INNER_RADIUS,
                    ToolTipLabelFormatter = _ => $"{MemoryUnitConverter.BytesToAutoString(process.WorkingSetBytes)}",
                    Stroke = new SolidColorPaint(new SKColor(255, 255, 255), 2.5f)
                });
            }

            if (otherProcessesMemoryBytes > 0)
            {
                double otherGb = MemoryUnitConverter.BytesToGB(otherProcessesMemoryBytes);
                if (otherGb > 0)
                {
                    /*series.Add(new PieSeries<double>
                    {
                        Values = new[] { otherGb },
                        Name = "Остальные процессы",
                        Fill = new SolidColorPaint(ProcessColors[5]),
                        HoverPushout = 4,
                        InnerRadius = DONUT_INNER_RADIUS,
                        ToolTipLabelFormatter = _ => $"{MemoryUnitConverter.BytesToAutoString(otherProcessesMemoryBytes)}",
                        Stroke = new SolidColorPaint(new SKColor(255, 255, 255), 2.5f)
                    });*/
                }
            }

            ProcessMemorySeries = series.ToArray();

            OnPropertyChanged(nameof(ProcessMemorySeries));
            OnPropertyChanged(nameof(TopProcesses));
        }

        private void AddAvailableMemorySlice(List<ISeries> series, ulong availableMemoryBytes)
        {
            double availableGb = MemoryUnitConverter.BytesToGB(availableMemoryBytes);
            if (availableGb <= 0) return;

            series.Add(new PieSeries<double>
            {
                Values = new[] { availableGb },
                Name = "Доступная память",
                Fill = new SolidColorPaint(ProcessColors[6]),
                HoverPushout = 4,
                InnerRadius = DONUT_INNER_RADIUS,
                ToolTipLabelFormatter = _ => $"{MemoryUnitConverter.BytesToAutoString(availableMemoryBytes)}",
                Stroke = new SolidColorPaint(new SKColor(255, 255, 255), 2.5f)
            });
        }

        private string FormatProcessName(string processName, int processId)
        {
            string name = processName?.Replace(".exe", "", StringComparison.OrdinalIgnoreCase) ?? "Unknown";

            const int maxLength = 15;
            if (name.Length > maxLength)
            {
                name = name.Substring(0, maxLength - 3) + "...";
            }

            return $"{name} ({processId})";
        }

        private static ulong ToUnsignedBytes(long value) =>
            (ulong)Math.Max(value, 0L);

        private static SolidColorBrush CreateBrush(SKColor color)
        {
            var brush = new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue));
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }
    }
}