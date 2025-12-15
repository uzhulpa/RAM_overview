using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAM_Overview.Services.Converters
{
    public static class MemoryUnitConverter
    {
        private const double KB_DIVISOR = 1024.0;
        private const double MB_DIVISOR = 1024.0 * 1024.0;
        private const double GB_DIVISOR = 1024.0 * 1024.0 * 1024.0;
        private const double TB_DIVISOR = 1024.0 * 1024.0 * 1024.0 * 1024.0;

        /// <summary>
        /// Конвертирует байты в килобайты (округление до 1 знака)
        /// </summary>
        public static double BytesToKB(ulong bytes)
        {
            return Math.Round(bytes / KB_DIVISOR, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Конвертирует байты в мегабайты (округление до 1 знака)
        /// </summary>
        public static double BytesToMB(ulong bytes)
        {
            return Math.Round(bytes / MB_DIVISOR, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Конвертирует байты в гигабайты (округление до 1 знака)
        /// </summary>
        public static double BytesToGB(ulong bytes)
        {
            return Math.Round(bytes / GB_DIVISOR, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Конвертирует байты в килобайты и возвращает строку (округление до 1 знака)
        /// </summary>
        public static string BytesToKBString(ulong bytes)
        {
            double kb = BytesToKB(bytes);
            return $"{kb.ToString("F1", CultureInfo.InvariantCulture)} КБ";
        }

        /// <summary>
        /// Конвертирует байты в мегабайты и возвращает строку (округление до 1 знака)
        /// </summary>
        public static string BytesToMBString(ulong bytes)
        {
            double mb = BytesToMB(bytes);
            return $"{mb.ToString("F1", CultureInfo.InvariantCulture)} МБ";
        }

        /// <summary>
        /// Конвертирует байты в гигабайты и возвращает строку (округление до 1 знака)
        /// </summary>
        public static string BytesToGBString(ulong bytes)
        {
            double gb = BytesToGB(bytes);
            return $"{gb.ToString("F1", CultureInfo.InvariantCulture)} ГБ";
        }

        /// <summary>
        /// Конвертирует байты в максимально емкую величину со значением не меньше 1
        /// </summary>
        public static string BytesToAutoString(ulong bytes)
        {
            if (bytes < KB_DIVISOR)
            {
                return $"{bytes} байт";
            }

            double kb = bytes / KB_DIVISOR;
            if (kb < 1.0)
            {
                return "1,0 КБ";
            }
            else if (kb < 1024.0)
            {
                double roundedKb = Math.Round(kb, 1, MidpointRounding.AwayFromZero);
                if (roundedKb < 1.0)
                    return "1,0 КБ";

                return $"{roundedKb.ToString("F1", CultureInfo.InvariantCulture)} КБ".Replace(".", ",");
            }

            double mb = bytes / MB_DIVISOR;
            if (mb < 1.0)
            {
                return "1,0 МБ";
            }
            else if (mb < 1024.0)
            {
                double roundedMb = Math.Round(mb, 1, MidpointRounding.AwayFromZero);
                if (roundedMb < 1.0)
                    return "1,0 МБ";

                return $"{roundedMb.ToString("F1", CultureInfo.InvariantCulture)} МБ".Replace(".", ",");
            }

            double gb = bytes / GB_DIVISOR;
            double roundedGb = Math.Round(gb, 1, MidpointRounding.AwayFromZero);
            if (roundedGb < 1.0)
                return "1,0 ГБ";

            return $"{roundedGb.ToString("F1", CultureInfo.InvariantCulture)} ГБ".Replace(".", ",");
        }

        public static string BytesToAutoString(long bytes)
        {
            if (bytes < KB_DIVISOR)
            {
                return $"{bytes} байт";
            }

            double kb = bytes / KB_DIVISOR;
            if (kb < 1.0)
            {
                return "1,0 КБ";
            }
            else if (kb < 1024.0)
            {
                double roundedKb = Math.Round(kb, 1, MidpointRounding.AwayFromZero);
                if (roundedKb < 1.0)
                    return "1,0 КБ";

                return $"{roundedKb.ToString("F1", CultureInfo.InvariantCulture)} КБ".Replace(".", ",");
            }

            double mb = bytes / MB_DIVISOR;
            if (mb < 1.0)
            {
                return "1,0 МБ";
            }
            else if (mb < 1024.0)
            {
                double roundedMb = Math.Round(mb, 2, MidpointRounding.AwayFromZero);
                if (roundedMb < 1.0)
                    return "1,0 МБ";

                return $"{roundedMb.ToString("F2", CultureInfo.InvariantCulture)} МБ".Replace(".", ",");
            }

            double gb = bytes / GB_DIVISOR;
            if (gb < 1.0)
            {
                return "1,0 ГБ";
            }
            else if (gb < 1024.0)
            {
                double roundedGb = Math.Round(gb, 2, MidpointRounding.AwayFromZero);
                if (roundedGb < 1.0)
                    return "1,0 ГБ";

                return $"{roundedGb.ToString("F2", CultureInfo.InvariantCulture)} ГБ".Replace(".", ",");
            }

            double tb = bytes / TB_DIVISOR;
            if (tb < 1.0) { return "1,0 ТБ"; }
            double roundedTb = Math.Round(tb, 2, MidpointRounding.AwayFromZero);
            if (roundedTb < 1.0) { return "1,0 ТБ"; }
            return $"{roundedTb.ToString("F2", CultureInfo.InvariantCulture)} ТБ".Replace(".", ",");
        }

        /// <summary>
        /// Конвертирует байты в максимально емкую величину с компактным форматом
        /// </summary>
        public static string BytesToCompactString(ulong bytes)
        {
            return BytesToAutoString(bytes);
        }
    }
}
