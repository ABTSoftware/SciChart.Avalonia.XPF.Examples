using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SciChart.Core.Utility;

namespace SciChart.Examples.Examples.PerformanceDemos2D.FifoBillionPoints
{
    public static class SystemMemoryInfo
    {
        private static long _physicalMemory = -1;

        public static long GetPhysicalMemoryGB()
        {
            if (_physicalMemory == -1)
            {
                if (SciChartRuntimeInfo.IsWindowsPlatform)
                {
                    _physicalMemory = GetWindowsPhysicalMemoryGB();
                }
                else if (SciChartRuntimeInfo.IsLinuxPlatform)
                {
                    _physicalMemory = GetLinuxPhysicalMemoryGB();
                }
            }
            return _physicalMemory;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

        private static long GetWindowsPhysicalMemoryGB()
        {
            if (GetPhysicallyInstalledSystemMemory(out long kb))
            {
                return kb / 1024L / 1024L;
            }
            return -1;
        }

        private static long GetLinuxPhysicalMemoryGB()
        {
            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemTotal:"))
                {
                    // "MemTotal:   16384000 kB"
                    var parts = line.Split(' ').Select(p => p.Trim()).ToArray();
                    if (parts.Length >= 2 && long.TryParse(parts[1], out long kb))
                    {
                        return kb / 1024L / 1024L;
                    }
                }
            }
            return -1;
        }
    }
}