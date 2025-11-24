using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using RAM_Overview.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAM_Overview.ViewModels
{
    public partial class ProcessMonitorViewModel : ObservableObject
    {
        private readonly ProcessMonitorService _processMonitorService;

        public ObservableCollection<ProcessModel> ProcessModels => _processMonitorService.ProcessModels;
        [ObservableProperty] private ProcessModel _selectedProcessModel;

        public ProcessMonitorViewModel(ProcessMonitorService processMonitorService)
        {
            _processMonitorService = processMonitorService;
        }
    }
}
