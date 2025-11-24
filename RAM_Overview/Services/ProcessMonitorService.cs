using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace RAM_Overview.Services
{
    public partial class ProcessMonitorService : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ProcessModel> _processModels = new();

        private readonly Timer _timer;

        private int _id = 0;

        public ProcessMonitorService()
        {
            _timer = new Timer(UpdateData, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void UpdateData(object? state)
        {
            var newProcessModelsList = new ObservableCollection<ProcessModel>();

            foreach (var proc in Process.GetProcesses())
            {
                var processModel = new ProcessModel
                {
                    Id = proc.Id,
                    Name = proc.ProcessName,
                    WorkingSetBytes = proc.WorkingSet64, // Обычно безопасно
                    PrivateMemoryBytes = proc.PrivateMemorySize64, // Обычно безопасно
                    PeakWorkingSetBytes = proc.PeakWorkingSet64, // Обычно безопасно
                    VirtualMemoryBytes = proc.VirtualMemorySize64, // Обычно безопасно
                    PagedMemoryBytes = proc.PagedMemorySize64, // Обычно безопасно
                    NonpagedMemoryBytes = proc.NonpagedSystemMemorySize64, // Обычно безопасно
                    BasePriority = proc.BasePriority, // Обычно безопасно
                };

                // БЕЗОПАСНОЕ заполнение опасных свойств
                try { processModel.ProcessFileName = proc.MainModule?.FileName ?? "N/A"; }
                catch { processModel.ProcessFileName = "Access Denied"; }

                try { processModel.StartTime = proc.StartTime; }
                catch { processModel.StartTime = DateTime.MinValue; }

                try { processModel.ThreadsCount = proc.Threads.Count; }
                catch { processModel.ThreadsCount = 0; }

                try { processModel.HandleCount = proc.HandleCount; }
                catch { processModel.HandleCount = 0; }

                // Вычисляемые свойства
                processModel.RunningTime = processModel.StartTime != DateTime.MinValue
                    ? DateTime.Now - processModel.StartTime
                    : TimeSpan.Zero;

                // Дополнительные сервисы (тоже могут падать)
                try { processModel.ProcessOwner = ProcessOwnerService.GetProcessOwner(processModel.Id); }
                catch { processModel.ProcessOwner = "N/A"; }

                try { processModel.CertificateIssuer = ProcessSignatureService.GetSignatureIssuer(processModel.ProcessFileName); }
                catch { processModel.CertificateIssuer = "N/A"; }

                newProcessModelsList.Add(processModel);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                ProcessModels.Clear();
                foreach (var processModel in newProcessModelsList)
                {
                    try
                    {
                        processModel.Icon = ProcessIconService.GetFileIcon(processModel.ProcessFileName);
                    }
                    catch
                    {
                    }

                    ProcessModels.Add(processModel);
                }
            });
        }

        public static string GetProcessOwnerWmi(int processId)
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT * FROM Win32_Process WHERE ProcessId = {processId}");

            using var results = searcher.Get();
            foreach (System.Management.ManagementObject process in results)
            {
                string[] ownerInfo = new string[2];
                process.InvokeMethod("GetOwner", ownerInfo);
                return $"{ownerInfo[1]}\\{ownerInfo[0]}"; // Domain\User
            }

            return "N/A";
        }


    }
}
