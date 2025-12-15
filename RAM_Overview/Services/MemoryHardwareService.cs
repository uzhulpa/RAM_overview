using CommunityToolkit.Mvvm.ComponentModel;
using RAM_Overview.Models;
using System.Management;

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
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                using var modules = searcher.Get();

                foreach (ManagementObject module in modules)
                {
                    try
                    {
                        var moduleInfo = new MemoryModuleInfo
                        {
                            CapacityBytes = Convert.ToUInt64(module["Capacity"]),
                            SpeedMHz = Convert.ToUInt32(module["ConfiguredClockSpeed"] ?? module["Speed"] ?? 0),
                            Manufacturer = module["Manufacturer"]?.ToString() != "Unknown" ? module["Manufacturer"]?.ToString() : "нет данных",
                            SerialNumber = module["SerialNumber"]?.ToString() ?? "нет данных",
                            MemoryType = GetMemoryTypeSMBIOS(module["SMBIOSMemoryType"]?.ToString()),
                            FormFactor = GetFormFactor(module["FormFactor"]?.ToString()),
                        };
                        MemoryInfo.Modules.Add(moduleInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки модуля памяти: {ex.Message}");
                        continue;
                    }
                }

                MemoryInfo.SlotsUsed = (uint)MemoryInfo.Modules.Count;
                MemoryInfo.TotalPhysicalMemoryBytes = (ulong)MemoryInfo.Modules.Sum(m => (long)m.CapacityBytes);

                using var memoryArray = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray");
                foreach (ManagementObject array in memoryArray.Get())
                {
                    MemoryInfo.TotalSlots += Convert.ToUInt32(array["MemoryDevices"]);
                }

                MemoryInfo.HardwareReservedBytes = GetHardwareReservedMemory(MemoryInfo);

                if (MemoryInfo.Modules.Any())
                {
                    MemoryInfo.SpeedMTPS = (uint)MemoryInfo.Modules.Average(m => m.SpeedMHz);
                    MemoryInfo.MemoryType = MemoryInfo.Modules.First().MemoryType;
                    MemoryInfo.FormFactor = MemoryInfo.Modules.First().FormFactor;
                    if (MemoryInfo.MemoryType.Contains("LPDDR")) MemoryInfo.FormFactor = "ряд микросхем";
                    MemoryInfo.SerialNumber = MemoryInfo.Modules.First().SerialNumber;
                    MemoryInfo.Manufacturer = string.Join(", ", MemoryInfo.Modules.Select(m => m.Manufacturer).Distinct());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения данных памяти: {ex.Message}");
            }
        }

        private string GetMemoryTypeSMBIOS(string memoryTypeCode)
        {
            if (string.IsNullOrEmpty(memoryTypeCode)) return "Unknown";
            if (!int.TryParse(memoryTypeCode, out int typeCode)) return "Invalid format";

            return typeCode switch
            {
                // Стандартные типы памяти
                0x01 => "Other",
                0x02 => "неизвестно",
                0x03 => "DRAM",
                0x04 => "EDRAM",
                0x05 => "VRAM",
                0x06 => "SRAM",
                0x07 => "RAM",
                0x08 => "ROM",
                0x09 => "FLASH",
                0x0A => "EEPROM",
                0x0B => "FEPROM",
                0x0C => "EPROM",
                0x0D => "CDRAM",
                0x0E => "3DRAM",
                0x0F => "SDRAM",
                0x10 => "SGRAM",
                0x11 => "RDRAM",
                0x12 => "DDR",
                0x13 => "DDR2",
                0x14 => "DDR2 FB-DIMM",
                0x15 => "Reserved",
                0x16 => "Reserved",
                0x17 => "Reserved",
                0x18 => "DDR3",
                0x19 => "FBD2",
                0x1A => "DDR4",
                0x1B => "LPDDR",
                0x1C => "LPDDR2",
                0x1D => "LPDDR3",
                0x1E => "LPDDR4",

                0x1F => "Logical non-volatile device",
                0x20 => "HBM",
                0x21 => "HBM2",
                0x22 => "DDR5",
                0x23 => "LPDDR5",
                0x24 => "HBM3",

                0x25 => "DDR5 NVDIMM-P",
                0x26 => "LPDDR5X",

                0xFE => "Controller-specific",
                0xFF => "Manufacturer-specific",

                _ when typeCode >= 0x27 && typeCode <= 0xFD => $"зарезервировано (0x{typeCode:X2})",
                _ => $"неизвестно (0x{typeCode:X2})"
            };
        }

        private string GetFormFactor(string formFactorCode)
        {
            if (string.IsNullOrEmpty(formFactorCode)) return "неизвестно";
            if (!int.TryParse(formFactorCode, out int typeCode)) return "некорректный формат";

            return typeCode switch
            {
                0 => "неизвестно",
                1 => "Other",
                2 => "SIP",
                3 => "DIP",
                4 => "ZIP",
                5 => "SOJ",
                6 => "Proprietary",
                7 => "SIMM",
                8 => "DIMM",
                9 => "TSOP",
                10 => "PGA",
                11 => "RIMM",
                12 => "SODIMM",
                13 => "SRIMM",
                14 => "SMD",
                15 => "SSMP",
                16 => "QFP",
                17 => "TQFP",
                18 => "SOIC",
                19 => "LCC",
                20 => "PLCC",
                21 => "BGA",
                22 => "FPBGA",
                23 => "LGA",
                24 => "FB-DIMM",
                _ => $"неизвестно ({typeCode})"
            };
        }

        private ulong GetHardwareReservedMemory(MemoryHardwareInfo info)
        {
            try
            {
                using var osSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject os in osSearcher.Get())
                {
                    ulong totalVisibleMemory = Convert.ToUInt64(os["TotalVisibleMemorySize"]) * 1024;
                    if (info.TotalPhysicalMemoryBytes > totalVisibleMemory)
                        return info.TotalPhysicalMemoryBytes - totalVisibleMemory;
                }
            }
            catch
            {
                return (ulong)(info.TotalPhysicalMemoryBytes * 0.05);
            }
            return 0;
        }
    }
}