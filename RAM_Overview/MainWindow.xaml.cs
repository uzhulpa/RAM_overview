using RAM_Overview.Services;
using RAM_Overview.ViewModels;
using System.Windows;

namespace RAM_Overview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var processMonitorService = new ProcessMonitorService();
            var memoryHardwareService = new MemoryHardwareService();
            var memoryInfoService = new MemoryAllocationInfoService();

            DataContext = new MainViewModel(processMonitorService, memoryHardwareService, memoryInfoService);
        }
    }   
}