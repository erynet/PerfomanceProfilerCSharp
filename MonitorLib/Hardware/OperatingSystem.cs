using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonitorLib.Hardware {
  internal static class OperatingSystem {

    public static bool Is64BitOperatingSystem() {
      if (IntPtr.Size == 8)
        return true;

      try {
        bool wow64Process;
        bool result = IsWow64Process(
          Process.GetCurrentProcess().Handle, out wow64Process);

        return result && wow64Process;
      } catch (EntryPointNotFoundException) {
        return false;
      }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWow64Process(IntPtr hProcess,
      out bool wow64Process);
  }
}
