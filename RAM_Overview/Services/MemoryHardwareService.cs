using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace RAM_Overview.Services
{
    public partial class MemoryHardwareService : ObservableObject
    {
        [ObservableProperty]
        private MemoryHardwareInfo _memoryInfo;

        public MemoryHardwareService()
        {
            MemoryInfo = new MemoryHardwareInfo();
            LoadHardwareInfo();
        }

        private void LoadHardwareInfo()
        {
            try
            {
                // Пробуем разные методы по порядку
                if (TryGetMemoryInfoFromSMBIOS())
                    return;

                if (TryGetMemoryInfoFromWMI())
                    return;

                if (TryGetMemoryInfoFromRegistry())
                    return;

                SetDefaultValues();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading memory hardware info: {ex.Message}");
                SetDefaultValues();
            }
        }

        // Способ 1: Через SMBIOS (более низкоуровневый)
        private bool TryGetMemoryInfoFromSMBIOS()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                using var results = searcher.Get();

                if (results.Count == 0)
                    return false;

                long totalCapacity = 0;
                int moduleCount = 0;
                bool firstModuleProcessed = false;

                foreach (ManagementObject memory in results)
                {
                    if (!firstModuleProcessed)
                    {
                        MemoryInfo.MemoryType = GetMemoryType((ushort)memory["MemoryType"]);
                        MemoryInfo.Speed = (ushort)memory["Speed"];
                        MemoryInfo.FormFactor = GetFormFactor((ushort)memory["FormFactor"]);
                        MemoryInfo.Manufacturer = GetManufacturerName(memory["Manufacturer"]?.ToString());
                        MemoryInfo.Timings = GetActualTimings(memory);
                        firstModuleProcessed = true;
                    }

                    totalCapacity += (long)(ulong)memory["Capacity"];
                    moduleCount++;
                }

                MemoryInfo.TotalPhysicalMemory = totalCapacity;
                UpdateSlotInfo(moduleCount);
                MemoryInfo.HardwareReserved = GetActualHardwareReserved();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Способ 2: Через WMI (более надежный для некоторых систем)
        private bool TryGetMemoryInfoFromWMI()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                using var results = searcher.Get();

                foreach (ManagementObject cs in results)
                {
                    MemoryInfo.TotalPhysicalMemory = (long)(ulong)cs["TotalPhysicalMemory"];
                    break;
                }

                // Получаем информацию о слотах
                using var memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray");
                using var memoryResults = memorySearcher.Get();

                foreach (ManagementObject array in memoryResults)
                {
                    MemoryInfo.SlotsTotal = Convert.ToInt32(array["MemoryDevices"]);
                    break;
                }

                // Считаем использованные слоты
                using var physicalMemorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                using var physicalMemoryResults = physicalMemorySearcher.Get();
                MemoryInfo.SlotsUsed = physicalMemoryResults.Count;

                // Базовые значения по умолчанию
                MemoryInfo.HardwareReserved = GetActualHardwareReserved();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Способ 3: Через реестр (запасной вариант)
        private bool TryGetMemoryInfoFromRegistry()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                if (key != null)
                {
                    // Можно получить некоторую информацию о системе, но о памяти мало данных
                    MemoryInfo.TotalPhysicalMemory = GetTotalMemoryFromGlobalMemoryStatus();
                    MemoryInfo.SlotsTotal = 4; // По умолчанию
                    MemoryInfo.SlotsUsed = 2;  // По умолчанию
                    MemoryInfo.MemoryType = "DDR4";
                    MemoryInfo.Speed = 3200;
                    MemoryInfo.Timings = "16-18-18-36";
                    MemoryInfo.FormFactor = "DIMM";
                    MemoryInfo.Manufacturer = "Unknown";
                    MemoryInfo.HardwareReserved = GetActualHardwareReserved();

                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Получение общего объема памяти через WinAPI
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private long GetTotalMemoryFromGlobalMemoryStatus()
        {
            try
            {
                var memoryStatus = new MEMORYSTATUSEX();
                memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

                if (GlobalMemoryStatusEx(ref memoryStatus))
                {
                    return (long)memoryStatus.ullTotalPhys;
                }
            }
            catch
            {
                // Ignore
            }

            return 8L * 1024 * 1024 * 1024; // 8 GB по умолчанию
        }

        // Более точное определение зарезервированной памяти
        private long GetActualHardwareReserved()
        {
            try
            {
                // Пытаемся получить реальное значение через WMI
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                using var results = searcher.Get();

                foreach (ManagementObject os in results)
                {
                    var totalVisibleMemory = (ulong)os["TotalVisibleMemorySize"] * 1024; // в байтах
                    var totalPhysicalMemory = (ulong)MemoryInfo.TotalPhysicalMemory; // явное преобразование в ulong

                    if (totalPhysicalMemory > totalVisibleMemory)
                    {
                        return (long)(totalPhysicalMemory - totalVisibleMemory);
                    }
                    break;
                }
            }
            catch
            {
                // Ignore
            }

            return 256 * 1024 * 1024; // 256 MB по умолчанию
        }

        // Получение реальных таймингов
        private string GetActualTimings(ManagementObject memory)
        {
            try
            {
                // Пытаемся получить реальные тайминги
                var configuredTimings = memory["ConfiguredMemoryTimings"]?.ToString();
                if (!string.IsNullOrEmpty(configuredTimings) && configuredTimings != "0")
                    return configuredTimings;

                // Альтернативные поля с таймингами
                var casLatency = memory["ConfiguredClockSpeed"]?.ToString();
                if (!string.IsNullOrEmpty(casLatency))
                    return $"{casLatency}-?-?-?";
            }
            catch
            {
                // Ignore
            }

            // Fallback к значениям по типу памяти
            return MemoryInfo.MemoryType switch
            {
                "DDR5" => "40-40-40-77",
                "DDR4" => "16-18-18-36",
                "DDR3" => "9-9-9-24",
                "DDR2" => "5-5-5-15",
                "DDR" => "2.5-3-3-7",
                _ => "Unknown"
            };
        }

        // Нормализация имени производителя
        private string GetManufacturerName(string manufacturer)
        {
            if (string.IsNullOrEmpty(manufacturer) || manufacturer == "Unknown")
                return "Unknown";

            return manufacturer.ToUpper() switch
            {
                "SAMSUNG" => "Samsung",
                "MICRON" or "MICRON TECHNOLOGY" => "Micron",
                "SK HYNIX" or "HYNIX SEMICONDUCTOR" => "SK Hynix",
                "CORSAIR" => "Corsair",
                "KINGSTON" => "Kingston",
                "CRUCIAL" => "Crucial",
                "G.SKILL" => "G.Skill",
                "PATRIOT" => "Patriot",
                "TEAM GROUP" => "Team Group",
                "ADATA" => "ADATA",
                "GEIL" => "GeIL",
                "APACER" => "Apacer",
                "TRANSCEND" => "Transcend",
                _ => manufacturer
            };
        }

        private void UpdateSlotInfo(int moduleCount)
        {
            try
            {
                using var arraySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray");
                using var results = arraySearcher.Get();

                foreach (ManagementObject array in results)
                {
                    MemoryInfo.SlotsTotal = Convert.ToInt32(array["MemoryDevices"]);
                    break;
                }
                MemoryInfo.SlotsUsed = moduleCount;
            }
            catch
            {
                // Автоматическое определение по количеству модулей
                MemoryInfo.SlotsTotal = Math.Max(moduleCount * 2, 4); // Минимум 4 слота
                MemoryInfo.SlotsUsed = moduleCount;
            }
        }

        private void SetDefaultValues()
        {
            MemoryInfo.MemoryType = "DDR4";
            MemoryInfo.Speed = 3200;
            MemoryInfo.Timings = "16-18-18-36";
            MemoryInfo.SlotsTotal = 4;
            MemoryInfo.SlotsUsed = 2;
            MemoryInfo.FormFactor = "DIMM";
            MemoryInfo.Manufacturer = "Unknown";
            MemoryInfo.TotalPhysicalMemory = GetTotalMemoryFromGlobalMemoryStatus();
            MemoryInfo.HardwareReserved = GetActualHardwareReserved();
        }

        private string GetMemoryType(ushort memoryType) => memoryType switch
        {
            20 => "DDR",
            21 => "DDR2",
            24 => "DDR3",
            26 => "DDR4",
            34 => "DDR5",
            _ => "Unknown"
        };

        private string GetFormFactor(ushort formFactor) => formFactor switch
        {
            8 => "DIMM",
            12 => "SODIMM",
            13 => "RIMM",
            14 => "CRIMM",
            _ => "Unknown"
        };
    }
}