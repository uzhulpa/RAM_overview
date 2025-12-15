using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RAM_Overview.Services
{
    public static class ProcessOwnerService
    {
        #region WinAPI Imports

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass,
            IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool ConvertSidToStringSid(IntPtr sid, out IntPtr stringSid);

        private const uint TOKEN_QUERY = 0x0008;
        private const uint ERROR_INSUFFICIENT_BUFFER = 122;

        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            TokenIsAppContainer,
            TokenCapabilities,
            TokenAppContainerSid,
            TokenAppContainerNumber,
            TokenUserClaimAttributes,
            TokenDeviceClaimAttributes,
            TokenRestrictedUserClaimAttributes,
            TokenRestrictedDeviceClaimAttributes,
            TokenDeviceGroups,
            TokenRestrictedDeviceGroups,
            TokenSecurityAttributes,
            TokenIsRestricted,
            TokenProcessTrustLevel,
            TokenPrivateNameSpace,
            TokenSingletonAttributes,
            TokenBnoIsolation,
            TokenChildProcessFlags,
            TokenIsLessPrivilegedAppContainer,
            TokenIsSandboxed,
            TokenOriginatingProcessTrustLevel
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Получает владельца процесса по его ID
        /// </summary>
        /// <param name="processId">ID процесса</param>
        /// <returns>Имя пользователя в формате DOMAIN\User или "N/A" при ошибке</returns>
        public static string GetProcessOwner(int processId)
        {
            IntPtr processHandle = IntPtr.Zero;
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                using var process = Process.GetProcessById(processId);
                processHandle = process.Handle;

                if (!OpenProcessToken(processHandle, TOKEN_QUERY, out tokenHandle))
                {
                    var error = Marshal.GetLastWin32Error();
                    return GetErrorMessage(error);
                }

                return GetTokenUser(tokenHandle);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("is not running"))
            {
                return "процесс не найден";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting process owner for PID {processId}: {ex.Message}");
                return "нет доступа";
            }
            finally
            {
                // Закрываем handles
                if (tokenHandle != IntPtr.Zero)
                    CloseHandle(tokenHandle);
            }
        }

        /// <summary>
        /// Получает владельца для текущего процесса
        /// </summary>
        public static string GetCurrentProcessOwner()
        {
            return GetProcessOwner(Process.GetCurrentProcess().Id);
        }

        #endregion

        #region Private Methods

        private static string GetTokenUser(IntPtr tokenHandle)
        {
            IntPtr tokenInfo = IntPtr.Zero;

            try
            {
                uint tokenInfoLength = 0;
                GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser,
                    IntPtr.Zero, 0, out tokenInfoLength);

                if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                    return "Failed to get token info size";

                tokenInfo = Marshal.AllocHGlobal((int)tokenInfoLength);

                if (!GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser,
                    tokenInfo, tokenInfoLength, out tokenInfoLength))
                {
                    return "Failed to get token info";
                }

                var tokenUser = Marshal.PtrToStructure<TOKEN_USER>(tokenInfo);

                return ConvertSidToUserName(tokenUser.User.Sid);
            }
            finally
            {
                if (tokenInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(tokenInfo);
            }
        }

        private static string ConvertSidToUserName(IntPtr sidPtr)
        {
            try
            {
                var sid = new SecurityIdentifier(sidPtr);
                var account = sid.Translate(typeof(NTAccount)) as NTAccount;
                return account?.Value ?? $"SID: {sid.Value}";
            }
            catch (IdentityNotMappedException)
            {
                try
                {
                    var sid = new SecurityIdentifier(sidPtr);
                    return $"SID: {sid.Value}";
                }
                catch
                {
                    return "Unknown SID";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting SID to username: {ex.Message}");
                return "SID conversion error";
            }
        }

        private static string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                5 => "нет доступа", // ERROR_ACCESS_DENIED
                87 => "некорректный параметр", // ERROR_INVALID_PARAMETER
                1008 => "процесс завершен", // ERROR_NO_TOKEN
                _ => $"код ошибки: {errorCode}"
            };
        }

        #endregion
    }
}
