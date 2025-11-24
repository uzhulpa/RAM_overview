using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RAM_Overview.Services
{
    public static class ProcessIconService
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInstance, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource GetProcessIcon(string processPath)
        {
            if (string.IsNullOrEmpty(processPath) || !File.Exists(processPath))
                return GetDefaultIcon();

            try
            {
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, processPath, 0);
                if (hIcon != IntPtr.Zero)
                {
                    var icon = System.Drawing.Icon.FromHandle(hIcon);
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    DestroyIcon(hIcon);
                    return bitmapSource;
                }
            }
            catch
            {
                // В случае ошибки возвращаем иконку по умолчанию
            }

            return GetDefaultIcon();
        }

        public static ImageSource GetFileIcon(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch
            {
                // Обработка ошибок
            }
            return GetDefaultIcon();
        }

        private static ImageSource GetDefaultIcon()
        {
            // Возвращаем стандартную иконку приложения
            var icon = SystemIcons.Application;
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
