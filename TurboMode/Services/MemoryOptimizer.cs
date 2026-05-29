using TurboMode.Native;

namespace TurboMode.Services;

/// <summary>
/// Windows'un biriktirdiği standby (cached) memory'yi temizler.
/// Düşük-orta RAM'li sistemlerde 1-3 GB ekstra serbest RAM kazanılır.
/// Tamamen güvenli — Windows zaten gerektiğinde bu cache'i otomatik atar.
/// </summary>
public static class MemoryOptimizer
{
    public static long PurgeStandbyList()
    {
        long beforeFreeKb = GetFreePhysicalKb();

        // Önce çalışan tüm process'lerin working set'lerini boşalt
        TryNtCommand(NtApi.MemoryEmptyWorkingSets, NtApi.SE_INCREASE_QUOTA_NAME);
        // Sonra standby list'i temizle (asıl büyük kazanç)
        TryNtCommand(NtApi.MemoryPurgeStandbyList, NtApi.SE_PROF_SINGLE_PROCESS_NAME);

        long afterFreeKb = GetFreePhysicalKb();
        return Math.Max(0, afterFreeKb - beforeFreeKb) / 1024; // MB
    }

    private static void TryNtCommand(int command, string privilegeName)
    {
        if (!EnablePrivilege(privilegeName)) return;
        int cmd = command;
        NtApi.NtSetSystemInformation(
            command == NtApi.MemoryEmptyWorkingSets
                ? NtApi.SystemFileCacheInformation
                : NtApi.SystemMemoryListInformation,
            ref cmd, sizeof(int));
    }

    private static bool EnablePrivilege(string privilegeName)
    {
        if (!NtApi.OpenProcessToken(NtApi.GetCurrentProcess(),
            NtApi.TOKEN_ADJUST_PRIVILEGES | NtApi.TOKEN_QUERY, out var token))
            return false;
        try
        {
            if (!NtApi.LookupPrivilegeValue(null, privilegeName, out var luid)) return false;
            var tp = new NtApi.TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new NtApi.LUID_AND_ATTRIBUTES
                {
                    Luid = luid,
                    Attributes = NtApi.SE_PRIVILEGE_ENABLED,
                }
            };
            return NtApi.AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
        }
        finally
        {
            Native.NativeMethods.CloseHandle(token);
        }
    }

    private static long GetFreePhysicalKb()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var o in searcher.Get())
                return Convert.ToInt64(o["FreePhysicalMemory"]);
        }
        catch { }
        return 0;
    }
}
