using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RAM_Overview.Services
{
    public static class ProcessSignatureService
    {
        public static string GetSignatureIssuer(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Debug.WriteLine("Путь к файлу не указан");
                return "нет пути к файлу";
            }

            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Файл не существует: {filePath}");
                return "файл не найден";
            }

            try
            {
                var certificate = LoadCertificateFromFile(filePath);

                if (certificate != null)
                {
                    string issuer = certificate.Issuer;
                    certificate.Dispose();
                    return ParseIssuerName(issuer);
                }
                else
                {
                    return "не подписано";
                }
            }
            catch (CryptographicException cex)
            {
                Debug.WriteLine($"Криптографическая ошибка при проверке подписи файла {filePath}: {cex.Message}");
                return "криптографическая ошибка";
            }
            catch (UnauthorizedAccessException uex)
            {
                Debug.WriteLine($"Ошибка доступа к файлу {filePath}: {uex.Message}");
                return "нет доступа";
            }
            catch (IOException ioex)
            {
                Debug.WriteLine($"Ошибка ввода-вывода для файла {filePath}: {ioex.Message}");
                return "ошибка ввода-вывода";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Неожиданная ошибка при проверке подписи файла {filePath}: {ex.Message}");
                return "неизвестная ошибка";
            }
        }

        private static X509Certificate2? LoadCertificateFromFile(string filePath)
        {
            try
            {
                var certificate = X509Certificate2.CreateFromSignedFile(filePath);

                if (certificate != null)
                {
                    return new X509Certificate2(certificate);
                }
            }
            catch (CryptographicException)
            {
                Debug.WriteLine($"Файл не содержит цифровой подписи: {filePath}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании сертификата из файла {filePath}: {ex.Message}");
            }

            try
            {
                using var cert = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(filePath);
                if (cert != null && !string.IsNullOrEmpty(cert.Issuer))
                {
                    return new X509Certificate2(cert);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Резервный метод также не сработал для файла {filePath}: {ex.Message}");
            }

            return null;
        }

        private static string ParseIssuerName(string issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer))
                return "неизвестный издатель";

            try
            {
                var parts = issuer.Split(',');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                    {
                        return trimmed.Substring(3);
                    }
                    if (trimmed.StartsWith("O=", StringComparison.OrdinalIgnoreCase))
                    {
                        return trimmed.Substring(2); 
                    }
                }

                return issuer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка парсинга имени издателя: {ex.Message}");
                return issuer;
            }
        }
    }
}
