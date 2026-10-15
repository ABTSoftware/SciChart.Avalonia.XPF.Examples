// *************************************************************************************
// SCICHART® Copyright SciChart Ltd. 2011-2026. All rights reserved.
//
// Web:     http://www.scichart.com
// Support: support@scichart.com
// Sales:   sales@scichart.com
//
// SystemMemoryInfo.cs is part of the SCICHART® Examples. Permission is hereby granted
// to modify, create derivative works, distribute and publish any part of this source
// code whether for commercial, private or personal use.
//
// The SCICHART® examples are distributed in the hope that they will be useful, but
// without any warranty. It is provided "AS IS" without warranty of any kind, either
// expressed or implied.
// *************************************************************************************
using System.Runtime.InteropServices;
using SciChart.Core.Utility;
using System.IO;
using System.Linq;

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