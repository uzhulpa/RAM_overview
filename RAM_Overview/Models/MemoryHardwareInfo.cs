using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using RAM_Overview.Services.Converters;

namespace RAM_Overview.Models
{
    public class MemoryHardwareInfo
    {
        public string Manufacturer { get; set; } = "нет данных";
        public string MemoryType { get; set; } = "нет данных";
        public ulong TotalPhysicalMemoryBytes { get; set; }
        public ulong HardwareReservedBytes { get; set; }
        public uint SpeedMTPS { get; set; }
        public uint SlotsUsed { get; set; }
        public uint TotalSlots { get; set; }
        public string FormFactor { get; set; } = "нет данных";
        public string SerialNumber { get; set; } = "нет данных";
        public List<MemoryModuleInfo> Modules { get; set; } = new();

        public double TotalPhysicalMemoryGB => TotalPhysicalMemoryBytes / 1024.0 / 1024.0 / 1024.0;
        public double HardwareReservedMB => Math.Round(HardwareReservedBytes / 1024.0 / 1024.0, MidpointRounding.AwayFromZero);

        public string SpeedMTPSString => $"{SpeedMTPS} МТ/с";
        public string SlotsUsedString => $"{SlotsUsed} из {TotalSlots}";
        public string TotalPhysicalMemoryString => MemoryUnitConverter.BytesToAutoString(TotalPhysicalMemoryBytes);
        public string HardwareReservedString => MemoryUnitConverter.BytesToAutoString(HardwareReservedBytes);

        // 1ст
        // скорость, использовано гнезд, тип памяти, зарезервировано аппаратно

        // 2ст
        // производитель, форм фактор, серийный номер
    }

    public class MemoryModuleInfo
    {
        public string Manufacturer { get; set; } = "нет данных";
        public string SerialNumber { get; set; } = "нет данных";
        public ulong CapacityBytes { get; set; }
        public uint SpeedMHz { get; set; }
        public string MemoryType { get; set; } = "нет данных";
        public string FormFactor { get; set; } = "нет данных";
    }
}
