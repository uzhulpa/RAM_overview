using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RAM_Overview.Services
{
    public static class ProcessSignatureService
    {
        public static string GetSignatureIssuer(string filePath)
        {
            try
            {
                // Загрузка сертификата из подписанного файла
                var certificate = LoadCertificateFromFile(filePath);

                if (certificate != null)
                {
                    return certificate.Issuer;
                }
            }
            catch (Exception ex)
            {
                // Файл не подписан или ошибка доступа
                Debug.WriteLine("ИСКЛЮЧЕНИЕ СЕРТИФИКАТ");
                Debug.WriteLine(ex.Message);
            }

            return "CERTIFICATE_ISSUER";
        }

        private static X509Certificate2 LoadCertificateFromFile(string filePath)
        {
            try
            {
                using var cert = X509Certificate.CreateFromSignedFile(filePath);
                return new X509Certificate2(cert);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ИСКЛЮЧЕНИЕ ЗАГРУЗКА СЕРТИФИКАТА");
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
