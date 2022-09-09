using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;

namespace essim_extension_core.Helpers
{
    public static class CompressionHelper
    {
        private static ILogger logger;

        public static void SetLogger(ILogger logHandler) => logger = logHandler;

        public static bool TryCompressFolder(string pathToFolder, out string pathToArchive, string [] extensionsFilter = null)
        {
            pathToArchive = null;
            if (!Directory.Exists(pathToFolder)) return false;

            DirectoryInfo directoryOfFilesToBeTarred = new DirectoryInfo(pathToFolder);
            FileInfo[] filesInDirectory = directoryOfFilesToBeTarred.GetFiles("*", SearchOption.AllDirectories);

            if (filesInDirectory.Length == 0) return false;

            try
            {
                pathToArchive = GetArchivePathForFolder(pathToFolder);
                using Stream targetStream = new GZipOutputStream(File.Create(pathToArchive));
                using TarArchive tarArchive = TarArchive.CreateOutputTarArchive(targetStream, TarBuffer.DefaultBlockFactor);
                foreach (FileInfo fileToBeTarred in filesInDirectory)
                {
                    if (extensionsFilter != null && !extensionsFilter.Contains(fileToBeTarred.Extension)) continue;
                    string entryName = fileToBeTarred.FullName.Replace(pathToFolder, string.Empty).TrimStart('\\', '/', ':');

                    TarEntry entry = TarEntry.CreateEntryFromFile(fileToBeTarred.FullName);
                    entry.Name = entryName;
                    tarArchive.WriteEntry(entry, true);
                }

                return true;
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to write to output {pathToArchive}\r\n{e.Message}\r\n{e.StackTrace}");
                return false;
            }
        }

        private static string GetArchivePathForFolder(string pathToFolder) => Path.Combine(pathToFolder, "essimOutput.tar.gz");

        public static void DecompressFile(string pathToFile, out string outputPath)
        {
            outputPath = pathToFile;
            if (string.IsNullOrEmpty(pathToFile)) return;
            
            FileInfo fileInfo = new FileInfo(pathToFile);
            if (!fileInfo.Exists) return;
            if (!fileInfo.Extension.Equals(".gz", StringComparison.InvariantCultureIgnoreCase)) return;

            try
            {
                outputPath = pathToFile.Substring(0, pathToFile.Length - 3); //Removes .gz

                using GZipInputStream inputStream = new GZipInputStream(fileInfo.OpenRead());
                using FileStream outputStream = File.Create(outputPath);
                inputStream.CopyTo(outputStream);
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to write to decompress {pathToFile}\r\n{e.Message}\r\n{e.StackTrace}");
                outputPath = null;
            }
        }
    }
}
