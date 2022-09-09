using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using essim_extension_core.Domain;
using essim_extension_core.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace essim_extension_core
{
    public static class AwsSqsClient
    {
        private static readonly ManualResetEvent StopSqsClient = new ManualResetEvent(false);
        private static ILogger logger;

        public static void SetLogger(ILogger logHandler) => logger = logHandler;
        
        public static async void ReadMessageFromSqs(Func<string, Action<QueueObject>, bool> contentHandler, Action<QueueObject> callback, Action readEnd)
        {
            try
            {
                ValidateEnvironmentVariables();

                MemoryMetrics memoryMetrics = new MemoryMetrics();
                if (memoryMetrics.PercentageUsed > 90.0)
                {
                    logger?.LogInformation($"Memory usage is {memoryMetrics.PercentageUsed:N2}%. No content new will be retrieved from SQS.");
                    readEnd?.Invoke();
                    return;
                }

                if (contentHandler == null)
                {
                    logger?.LogInformation("No content handler for content from SQS was specified");
                    readEnd?.Invoke();
                    return;
                }

                AmazonSQSClient sqsClient = AwsHelper.GetSqsClient();
                string queueUrl = Environment.GetEnvironmentVariable("AWS_ESSIM_QUEUE_URL");
                int queueTimeout = Convert.ToInt32(Environment.GetEnvironmentVariable("AWS_ESSIM_QUEUE_TIMEOUT"));

                Message message = GetMessageFromSqs(sqsClient, queueUrl, TimeSpan.FromSeconds(queueTimeout)).Result;

                if (message == null)
                {
                    logger?.LogInformation("No content was received from SQS");
                    readEnd?.Invoke();
                    return;
                }

                logger?.LogInformation("Successfully received content from SQS. Initiating handler");

                if (contentHandler(message.Body, callback))
                {
                    logger?.LogInformation("SQS content handler finished successfully. Deleting message from queue");
                    await DeleteMessage(sqsClient, message, queueUrl);
                }
                else
                {
                    logger?.LogWarning("SQS content handler failed");
                }
            }
            catch (Exception e)
            {
                logger?.LogError($"Failed to read message from SQS, task is cancelled.\r\n{e.Message}\r\n{e.StackTrace}");
            }
        }

        public static void WriteMessageToSqs(object queueObject)
        {
            if (queueObject == null) return;
            WriteMessageToSqs(JsonConvert.SerializeObject(queueObject, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore}));
        }

        public static async void WriteMessageToSqs(string body) 
        {
            ValidateEnvironmentVariables();

            AmazonSQSClient sqsClient = AwsHelper.GetSqsClient();
            string queueUrl = Environment.GetEnvironmentVariable("AWS_ESSIM_EXPORT_QUEUE_URL");

            await sqsClient.SendMessageAsync(queueUrl, body);
        }

        private static void ValidateEnvironmentVariables()
        {
            ValidateEnvironmentVariable("AWS_ESSIM_QUEUE_URL");
            ValidateEnvironmentVariableAsInteger("AWS_ESSIM_QUEUE_TIMEOUT");
            ValidateEnvironmentVariable("AWS_ESSIM_EXPORT_QUEUE_URL");
        }

        private static void ValidateEnvironmentVariable(string name)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                throw new NullReferenceException($"Environment variable '{name}' is not set!");
        }

        private static void ValidateEnvironmentVariableAsInteger(string name)
        {
            ValidateEnvironmentVariable(name);
            if (!Int32.TryParse(Environment.GetEnvironmentVariable(name), out _))
                throw new NullReferenceException($"Environment variable '{name}' is not of type Int32!");
        }

        private static async Task<Message> GetMessageFromSqs(AmazonSQSClient sqsClient, string queueUrl, TimeSpan timeout)
        {
            ReceiveMessageResponse response;
            DateTime expiry = DateTime.UtcNow.Add(timeout);

            logger?.LogInformation("Attempting to read content fom SQS");
            
            do
            {
                int taskTimeout = timeout.TotalSeconds > 5.0 ? 5 : Convert.ToInt32(timeout.TotalSeconds);
                response = await GetMessage(sqsClient, queueUrl, taskTimeout);
                if (response.Messages.Count == 0) continue;
                break;

            } while (!StopSqsClient.WaitOne(100) && DateTime.UtcNow < expiry);

            if (response.Messages.Count == 0)
                return null;

            return response.Messages[0];
        }

        private static async Task<ReceiveMessageResponse> GetMessage(IAmazonSQS sqsClient, string queueUrl, int waitTime = 0) => await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = waitTime
        });

        private static async Task DeleteMessage(IAmazonSQS sqsClient, Message message, string queueUrl) => await sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);

        public static void Stop()
        {
            StopSqsClient?.Set();
        }
    }
}
