using System;
using System.IO;
using essim_extension_core.Domain;

namespace essim_extension_core.Helpers
{
    public class StorageHelper
    {
        public static string GetPathToCsvStorage(QueueObject queueObject) => GetPathToCsvStorage(Environment.GetEnvironmentVariable("CSV_STORAGE_LOCATION"), queueObject);
        
        public static string GetPathToCsvStorage(string outputPath, QueueObject queueObject)
        {
            if (string.IsNullOrEmpty(outputPath) || 
                string.IsNullOrEmpty(queueObject?.ScenarioUuid) || 
                queueObject.ScenarioYear == null) return null;

            return Path.Combine(outputPath, queueObject.ScenarioUuid, queueObject.ScenarioYear.ToString());
        }

        public static void CleanUpFiles(QueueObject queueObject)
        {
            DeleteDirectory(GetPathToCsvStorage(queueObject));
        }

        private static void DeleteDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return;
                Directory.Delete(path, true);
            }
            catch
            {
                //
            }
        }
    }
}