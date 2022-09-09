using System;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;

namespace essim_extension_core.Helpers
{
    public class AwsHelper
    {
        public static AWSCredentials GetAwsCredentials()
        {
            string awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            string awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            string awsSessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");

            //Use locally stored AWS credentials
            if (string.IsNullOrEmpty(awsAccessKey) &&
                string.IsNullOrEmpty(awsSecretAccessKey) &&
                string.IsNullOrEmpty(awsSessionToken))
                return null;

            //Use basic AWS credentials
            if (!string.IsNullOrEmpty(awsAccessKey) &&
                !string.IsNullOrEmpty(awsSecretAccessKey) &&
                string.IsNullOrEmpty(awsSessionToken))
                return new BasicAWSCredentials(awsAccessKey, awsSecretAccessKey);
            
            //Use AWS session credentials
            return new SessionAWSCredentials(awsAccessKey, awsSecretAccessKey, awsSessionToken);
        }

        public static AmazonSQSClient GetSqsClient()
        {
            AWSCredentials credentials = GetAwsCredentials();
            return credentials == null ? new AmazonSQSClient(RegionEndpoint.EUCentral1) : new AmazonSQSClient(credentials, RegionEndpoint.EUCentral1);
        }

        public static AmazonS3Client GetS3Client()
        {
            AWSCredentials credentials = GetAwsCredentials();
            return credentials == null ? new AmazonS3Client(RegionEndpoint.EUCentral1) : new AmazonS3Client(credentials, RegionEndpoint.EUCentral1);
        }

        public static string GetStoragePathOnS3(string pathToFile)
        {
            string csvStorageLocation = Environment.GetEnvironmentVariable("CSV_STORAGE_LOCATION");
            if (!string.IsNullOrEmpty(csvStorageLocation) && pathToFile.Contains(csvStorageLocation))
                pathToFile = pathToFile.Replace(csvStorageLocation, string.Empty);

            string[] pathComponents = pathToFile.Split(new[] { '\\', '/', ':' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder s3PathBuilder = new StringBuilder();

            for (int i = 0; i < pathComponents.Length; i++)
            {
                s3PathBuilder.Append(pathComponents[i]);
                if (i == pathComponents.Length- 1) continue;
                s3PathBuilder.Append("/");
            }

            return s3PathBuilder.ToString();
        }
    }
}
