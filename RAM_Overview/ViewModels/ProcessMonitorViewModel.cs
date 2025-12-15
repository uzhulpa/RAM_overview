using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RAM_Overview.Models;
using RAM_Overview.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RAM_Overview.ViewModels
{
    public partial class ProcessMonitorViewModel : ObservableObject
    {
        private readonly ProcessMonitorService _processMonitorService;

        public ObservableCollection<ProcessModel> ProcessModels => _processMonitorService.ProcessModels;
        [ObservableProperty] private ProcessModel? _selectedProcessModel;

        [ObservableProperty] private int _selectedProcessId = Process.GetCurrentProcess().Id;

        public ProcessMonitorViewModel(ProcessMonitorService processMonitorService)
        {
            _processMonitorService = processMonitorService;

            _processMonitorService.ProcessModels.CollectionChanged += ProcessModels_CollectionChanged;

            SelectedProcessModel = ProcessModels.FirstOrDefault(p => p.Id == SelectedProcessId, null);
        }

        [RelayCommand]
        private void OpenProcessFileLocation()
        {
            if (SelectedProcessModel == null || string.IsNullOrEmpty(SelectedProcessModel.ProcessFileName) || SelectedProcessModel.ProcessFileName == "нет доступа")
            {
                MessageBox.Show("Не удалось определить путь к файлу процесса.", "Нет доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string filePath = SelectedProcessModel.ProcessFileName;

                if (File.Exists(filePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                else if (Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Process.Start("explorer.exe", Path.GetDirectoryName(filePath));
                }
                else
                {
                    MessageBox.Show("Файл или папка не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии папки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void TerminateProcess(int processId)
        {
            var process = ProcessModels.FirstOrDefault(p => p.Id == processId);
            if (process == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите завершить процесс \"{process.Name}\" (PID: {process.Id})?\n\n" +
                "Это может привести к потере несохраненных данных или нестабильной работе системы.",
                "Подтверждение завершения процесса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var systemProcess = Process.GetProcessById(processId);

                    if (processId == Process.GetCurrentProcess().Id)
                    {
                        MessageBox.Show("Нельзя завершить текущий процесс приложения.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (systemProcess.ProcessName.ToLower().Contains("system") ||
                        systemProcess.ProcessName.ToLower().Contains("svchost") ||
                        systemProcess.ProcessName.ToLower().Contains("winlogon") ||
                        systemProcess.ProcessName.ToLower().Contains("csrss") ||
                        systemProcess.ProcessName.ToLower().Contains("lsass"))
                    {
                        var systemResult = MessageBox.Show(
                            $"Процесс \"{process.Name}\" может быть системным процессом.\n" +
                            "Завершение системных процессов может привести к нестабильной работе системы.\n\n" +
                            "Вы уверены, что хотите продолжить?",
                            "Предупреждение о системном процессе",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (systemResult != MessageBoxResult.Yes)
                            return;
                    }

                    systemProcess.Kill();
                    systemProcess.WaitForExit(5000);

                    MessageBox.Show($"Процесс \"{process.Name}\" успешно завершен.", "Процесс завершен",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    SelectedProcessId = Process.GetCurrentProcess().Id;
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show($"Недостаточно прав для завершения процесса \"{process.Name}\".\n\n" +
                                  $"Ошибка: {ex.Message}", "Ошибка доступа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Процесс \"{process.Name}\" уже завершен или не существует.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось завершить процесс \"{process.Name}\".\n\n" +
                                  $"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        partial void OnSelectedProcessModelChanged(ProcessModel? value)
        {
            if (value == null) return;

            SelectedProcessId = value.Id;
        }

        private void ProcessModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SelectedProcessId == -1) return;

            var newProcess = ProcessModels.FirstOrDefault(p => p.Id == SelectedProcessId);
            if (newProcess != null) SelectedProcessModel = newProcess;
        }
    }
}