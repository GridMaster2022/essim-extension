using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace essim_extension_core.Domain
{
    public class MemoryMetrics
    {
        public double Total { get; private set; }
        public double Used { get; private set; }
        public double Free { get; private set; }
        public double PercentageUsed { get; private set; }
        public string Platform { get; private set; }

        public MemoryMetrics()
        {
            if (IsUnix())
                GetLinuxMetrics();
            else
                GetWindowsMetrics();
        }

        private bool IsUnix() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||                     
                                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private void GetWindowsMetrics()
        {
            try
            {
                Platform = "Windows";
                
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                    RedirectStandardOutput = true
                };

                string output = null;
                using (Process process = Process.Start(info))
                {            
                    output = process?.StandardOutput.ReadToEnd();       
                }

                if (string.IsNullOrEmpty(output))
                {
                    SetValuesToUnknown();
                    return;
                }

                string[] lines = output.Trim().Split("\n");
                string[] freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
                string[] totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

                Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);        
                Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);        
                Used = Total - Free;
                PercentageUsed = (Used / Total) * 100.0;
            }
            catch
            {
                SetValuesToUnknown();
            }
        }

        private void GetLinuxMetrics()
        {
            try
            {
                Platform = "Linux";
                
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = "/bin/cat",
                    Arguments = "/proc/meminfo",
                    RedirectStandardOutput = true
                };

                string output = null;
                using (Process process = Process.Start(info))
                {            
                    output = process?.StandardOutput.ReadToEnd();       
                }

                if (string.IsNullOrEmpty(output))
                {
                    SetValuesToUnknown();
                    return;
                }

                string[] lines = output.Trim().Split("\n");
                string[] freeMemoryParts = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string[] totalMemoryParts = lines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);        
                Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);        
                Used = Total - Free;
                PercentageUsed = (Used / Total) * 100.0;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERROR {e.Message}");
                SetValuesToUnknown();
            }
        }

        private void SetValuesToUnknown()
        {
            Total = 0.0;
            Used = 0.0;
            Free = 0.0;
            PercentageUsed = 100;
        }
    }
}
