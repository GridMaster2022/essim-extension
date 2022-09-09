using System;
using System.Diagnostics;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using essim_extension_core.Helpers;
using Microsoft.Extensions.Logging;

namespace essim_extension_core
{
    public static class AwsS3Client
    {
        private static ILogger logger;

        public static void SetLogger(ILogger logHandler) => logger = logHandler;
        
        public static string ReadFile(string bucketName, string pathToFile)
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(pathToFile))
                return null;
            
            try
            {
                AmazonS3Client s3Client = AwsHelper.GetS3Client();

                GetObjectRequest objectRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = pathToFile
                };

                using GetObjectResponse response = s3Client.GetObjectAsync(objectRequest).Result;
                using StreamReader reader = new StreamReader(response.ResponseStream);

                return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to read {pathToFile} from {bucketName}\r\n{e.Message}\r\n{e.StackTrace}");
                return null;
            }
        }

        public static bool DownloadFile(string bucketName, string pathToFile, string pathOnDisk)
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(pathToFile) || string.IsNullOrEmpty(pathOnDisk))
                return false;

            try
            {
                AmazonS3Client s3Client = AwsHelper.GetS3Client();
                TransferUtility transferUtility = new TransferUtility(s3Client);

                TransferUtilityDownloadRequest downloadRequest = new TransferUtilityDownloadRequest
                {
                    BucketName = bucketName,
                    Key = pathToFile,
                    FilePath = pathOnDisk
                };

                transferUtility.Download(downloadRequest);
                return true;
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to store {pathToFile} from {bucketName} in local file {pathOnDisk}\r\n{e.Message}\r\n{e.StackTrace}");
                return false;
            }
        }

        public static bool UploadFile(string bucketName, string pathToFile, string pathOnDisk)
        {
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(pathToFile) || string.IsNullOrEmpty(pathOnDisk))
                return false;
            
            try
            {
                AmazonS3Client s3Client = AwsHelper.GetS3Client();
                TransferUtility transferUtility = new TransferUtility(s3Client);

                TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    Key = pathToFile,
                    FilePath = pathOnDisk
                };

                transferUtility.Upload(uploadRequest);
                return true;
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to upload {pathOnDisk} to {bucketName} in {pathToFile}\r\n{e.Message}\r\n{e.StackTrace}");
                return false;
            }
        }
    }
}
