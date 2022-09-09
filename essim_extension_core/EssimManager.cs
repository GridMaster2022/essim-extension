using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using essim_extension_core.Helpers;
using Microsoft.Extensions.Logging;

namespace essim_extension_core
{
    public static class EssimManager
    {
        private static Process essimApplication;
        private static readonly ManualResetEvent StopManager = new ManualResetEvent(false);

        private static ILogger logger;

        public static bool ApplicationStarted
        {
            get
            {
                if (essimApplication == null) return false;
                if (essimApplication.HasExited) return false;
                return essimApplication.Responding;
            }
        }

        public static void SetLogger(ILogger logHandler) => logger = logHandler;

        public static bool ApplicationResponsive { get; private set; }

        public static string ApplicationUrl => $"{Environment.GetEnvironmentVariable("HTTP_SERVER_SCHEME")}://localhost:{Environment.GetEnvironmentVariable("HTTP_SERVER_PORT")}/{Environment.GetEnvironmentVariable("HTTP_SERVER_PATH")}/simulation";

        public static int ApplicationStartCount { get; private set; }

        static EssimManager()
        {
            ApplicationStartCount = -1;
            Task.Run(MonitorApplication);
        }

        public static void StartEssim()
        {
            if (ApplicationStarted) return;
            if (Environment.OSVersion.Platform != PlatformID.Unix) return;
            if (!File.Exists("/opt/essim.jar")) return;

            if (ApplicationStartCount != Int32.MaxValue) //Prevent overflow
                ApplicationStartCount++;

            if (essimApplication != null)
                logger?.LogWarning($"Essim application terminated unexpectedly with exitCode {essimApplication.ExitCode}");

            logger?.LogInformation("Starting Essim");

            try
            {
                essimApplication = Process.Start("java", "-Xms4096m -Xmx4096m -jar /opt/essim.jar");
            }
            catch (Exception e)
            {
                string errorCodeText = essimApplication?.ExitCode != null ? $" Application exitCode: {essimApplication.ExitCode}" : string.Empty;
                logger?.LogError($"Error while starting Essim application.{errorCodeText}\r\n{e.Message}\r\n{e.StackTrace}");
                essimApplication = null;
            }
        }

        private static void MonitorApplication()
        {
            while (!StopManager.WaitOne(10_000))
            {
                try
                {
                    ApplicationResponsive = EssimIsResponsive();
                    StartEssim();
                }
                catch (Exception e)
                {
                    logger?.LogError($"Error while monitoring Essim application.\r\n{e.Message}\r\n{e.StackTrace}");
                    throw;
                }
            }
        }

        private static bool EssimIsResponsive()
        {
            WebRequestHelper.ExecuteWebRequest(ApplicationUrl, "HEAD", null, null, out bool success, out HttpStatusCode? _);
            return success;
        }

        public static void Stop()
        {
            StopManager?.Set();
            essimApplication?.Kill(true);
        }
    }
}
