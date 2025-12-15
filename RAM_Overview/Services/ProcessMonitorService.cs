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

        public ProcessMonitorService()
        {
            _timer = new Timer(UpdateData, null, TimeSpan.Zero, TimeSpan.FromSeconds(1.5));
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
                    WorkingSetBytes = proc.WorkingSet64,
                    PrivateMemoryBytes = proc.PrivateMemorySize64,
                    PeakWorkingSetBytes = proc.PeakWorkingSet64,
                    VirtualMemoryBytes = proc.VirtualMemorySize64,
                    PeakVirtualMemoryBytes = proc.PeakVirtualMemorySize64,
                    PagedMemoryBytes = proc.PagedMemorySize64,
                    NonpagedMemoryBytes = proc.NonpagedSystemMemorySize64,
                    BasePriority = proc.BasePriority,
                };

                try { processModel.ProcessFileName = proc.MainModule?.FileName ?? "N/A"; }
                catch {
                    Debug.WriteLine($"Не удалось получить путь к файлу процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.ProcessFileName = "нет доступа"; }

                try { processModel.StartTime = proc.StartTime; }
                catch {
                    Debug.WriteLine($"Не удалось получить время запуска процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.StartTime = DateTime.MinValue; }

                try { processModel.ThreadsCount = proc.Threads.Count; }
                catch {
                    Debug.WriteLine($"Не удалось получить количество потоков процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.ThreadsCount = 0; }

                try { processModel.HandleCount = proc.HandleCount; }
                catch {
                    Debug.WriteLine($"Не удалось получить количество дескрипторов процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.HandleCount = 0; }

                processModel.RunningTime = processModel.StartTime != DateTime.MinValue
                    ? DateTime.Now - processModel.StartTime
                    : TimeSpan.Zero;

                try { processModel.ProcessOwner = ProcessOwnerService.GetProcessOwner((int)processModel.Id); }
                catch {
                    Debug.WriteLine($"Не удалось получить владельца процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.ProcessOwner = "нет доступа"; }

                try { processModel.CertificateIssuer = ProcessSignatureService.GetSignatureIssuer(processModel.ProcessFileName); }
                catch {
                    Debug.WriteLine($"Не удалось получить издателя сертификата процесса: {processModel.Name} (PID: {processModel.Id})");
                    processModel.CertificateIssuer = "нет доступа"; }

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
                        Debug.WriteLine($"Не удалось получить иконку процесса: {processModel.Name} (PID: {processModel.Id})");
                    }

                    ProcessModels.Add(processModel);
                }

                OnPropertyChanged(nameof(ProcessModels));
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

            return "нет доступа";
        }


    }
}
