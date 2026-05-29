using System.Runtime.InteropServices;

namespace TurboMode.Native;

/// <summary>
/// Süreç askıya alma için ntdll undocumented (ama yıllardır kararlı) çağrıları.
/// .NET'in Process.Suspend metodu yok; bu yüzden NtSuspendProcess/NtResumeProcess kullanıyoruz.
/// </summary>
internal static class NativeMethods
{
    [Flags]
    public enum ProcessAccess : uint
    {
        SuspendResume = 0x0800,
        QueryLimitedInformation = 0x1000,
        Terminate = 0x0001,
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(ProcessAccess access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("ntdll.dll", SetLastError = false)]
    public static extern int NtSuspendProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = false)]
    public static extern int NtResumeProcess(IntPtr processHandle);

    /// <summary>
    /// Process'in working set'ini boşaltır (RAM baskısını azaltır, sayfalama yapar).
    /// </summary>
    [DllImport("psapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EmptyWorkingSet(IntPtr hProcess);
}
