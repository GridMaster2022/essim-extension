using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using essim_extension_core.Domain;
using essim_extension_core.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace essim_extension_core
{
    public class SimulationProcessor
    {
        private static bool workingOnSimulation;
        private static string simulationId;
        private static string simulationDashboardUrl;
        private static double simulationProgress;
        private static string simulationStateValue;
        private static string simulationStateDescription;
        private static ManualResetEvent stopSimulation;
        private static QueueObject simulationQueueObject;
        private static Action<QueueObject> simulationFinishedHandler;
        private static ILogger logger;

        private static readonly object SyncRoot = new object();
        private static readonly Dictionary<string, string> RequestHeaders = new Dictionary<string, string>()
        {
            {"Content-Type", "application/json"},
            {"Accept", "application/json"}
        };

        public static string SimulationId => simulationId;
        public static string SimulationDashboardUrl => simulationDashboardUrl;
        public static double SimulationProgress => simulationProgress;
        public static string SimulationStateValue => simulationStateValue;
        public static string SimulationStateDescription => simulationStateDescription;

        public static void SetLogger(ILogger logHandler) => logger = logHandler;

        public static bool ProcessEsdlContent(string inputContent, Action<QueueObject> finishedHandler)
        {
            try
            {
                simulationQueueObject = JsonConvert.DeserializeObject<QueueObject>(inputContent);
            }
            catch (Exception e)
            {
                simulationQueueObject = null;
                logger?.LogError($"Failed to process input from queue - {inputContent}\r\n{e.Message}\r\n{e.StackTrace}");
            }

            SimulationRequest simulationRequest = SimulationRequestHelper.GetSimulationRequest(simulationQueueObject, logger);
            
            if (simulationRequest == null)
            {
                logger?.LogWarning("Failed to generate simulationRequest");
                return false;
            }

            lock (SyncRoot)
            {
                if (workingOnSimulation) return false;
                ResetSimulation(finishedHandler, simulationQueueObject);
                workingOnSimulation = true;
            }

            Task.Run(() => StartSimulation(simulationRequest));

            return true;
        }

        private static void ResetSimulation(Action<QueueObject> finishedHandler, QueueObject queueObject)
        {
            stopSimulation?.Set();
            lock (SyncRoot)
            {
                workingOnSimulation = false;
                simulationId = string.Empty;
                simulationDashboardUrl = string.Empty;
                simulationProgress = 0.0;
                simulationStateValue = "NOT STARTED";
                simulationStateDescription = string.Empty;
                simulationQueueObject = queueObject;
                simulationFinishedHandler = finishedHandler;
                stopSimulation = new ManualResetEvent(false);
            }
        }

        private static void StartSimulation(SimulationRequest request)
        {
            string jsonRequest = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-ddTHH:mm:ssK",
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            }).Replace("+00:00", "+0000").Replace("+01:00", "+0100").Replace("+02:00", "+0200");

            logger?.LogInformation($"Starting ESSIM Simulation for scenarioId {request.ScenarioId} ({request.GetOutputType()})");
            bool stayInWhileLoop = true;
            try
            {
                int errorCount = 0;

                do
                {
                    string responseContent = String.Empty;
                    StateResponse response = null;
                    HttpStatusCode? statusCode = null;

                    try
                    {
                        responseContent = WebRequestHelper.ExecuteWebRequest(EssimManager.ApplicationUrl, "POST", null, jsonRequest, out bool _, out statusCode, headers: RequestHeaders);
                        if (string.IsNullOrEmpty(responseContent)) continue;
                        if (statusCode == null) continue;
                        if (statusCode == HttpStatusCode.ServiceUnavailable || responseContent.Equals("Busy", StringComparison.InvariantCultureIgnoreCase)) continue;
                        response = JsonConvert.DeserializeObject<StateResponse>(responseContent);
                        if (response == null) continue;
                        errorCount = 0;
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        logger?.LogWarning($"Failed to request simulation state from ESSIM on {EssimManager.ApplicationUrl}.\r\n" +
                                                 $"HTTP status code: {statusCode}\r\n" +
                                                 $"Response content: {responseContent}\r\n" +
                                                 $"Exception: {e.Message}\r\n" +
                                                 $"{e.StackTrace}");

                        if (errorCount >= 3)
                        {
                            logger.LogError($"{errorCount} consecutive exceptions while monitoring simulation status. Exceptions will nog longer be accepted!");
                            throw;
                        }

                        continue;
                    }

                    switch (statusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Created:
                        case HttpStatusCode.Accepted:
                            stayInWhileLoop = false;
                            simulationId = response.Id;
                            simulationStateValue = "CREATED";

                            logger?.LogInformation($"Successfully started ESSIM Simulation with id {response.Id}");

                            simulationDashboardUrl = GetDashboardUrl();
                            Task.Run(MonitorProgress);
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            logger?.LogInformation("The ESSIM Engine is busy. Retrying in 5 seconds...");
                            break;
                        default:
                            stayInWhileLoop = false;

                            logger?.LogInformation($"ESSIM Simulation failed because: {response.Description}");

                            lock (SyncRoot)
                            {
                                workingOnSimulation = false;
                                simulationId = string.Empty;
                                simulationFinishedHandler?.Invoke(null);
                            }
                            break;
                    }
                } while (stayInWhileLoop && !stopSimulation.WaitOne(5_000));
            }
            catch (Exception e)
            {
                logger?.LogError($"Error while starting simulation. {e.Message}\r\n{e.StackTrace}");

                lock (SyncRoot)
                {
                    workingOnSimulation = false;
                    simulationId = string.Empty;
                }
            }
        }

        private static string GetDashboardUrl()
        {
            if (stopSimulation.WaitOne(0)) return string.Empty;
            if (string.IsNullOrEmpty(simulationId)) return string.Empty;

            string url = $"{EssimManager.ApplicationUrl}/{simulationId}";

            try
            {
                string responseContent = WebRequestHelper.ExecuteWebRequest(url, "GET", null, null, out bool _, out HttpStatusCode? statusCode, headers: RequestHeaders);

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        InformationResponse response = JsonConvert.DeserializeObject<InformationResponse>(responseContent);
                        if (string.IsNullOrEmpty(response?.DashboardUrl))
                        {
                            logger?.LogWarning($"Dashboard URL not found! Simulation meta-data looks like so:\r\n{responseContent}");
                            break;
                        }

                        logger?.LogInformation($"Dashboard URL: {response.DashboardUrl}");
                        return response.DashboardUrl;
                    default:
                        StateResponse errorResponse = JsonConvert.DeserializeObject<StateResponse>(responseContent);
                        logger?.LogWarning($"{errorResponse?.Description}");
                        break;
                }
            }
            catch (Exception e)
            {
                logger?.LogError($"Error while setting dashboard URL. {e.Message}\r\n{e.StackTrace}");
            }

            return string.Empty;
        }

        private static void MonitorProgress()
        {
            if (string.IsNullOrEmpty(simulationId)) return;

            string url = $"{EssimManager.ApplicationUrl}/{simulationId}/status";

            while (!stopSimulation.WaitOne(1_000))
            {
                int errorCount = 0;

                try
                {
                    string responseContent = String.Empty;
                    StateResponse response = null;
                    HttpStatusCode? statusCode = null;

                    try
                    {
                        responseContent = WebRequestHelper.ExecuteWebRequest(url, "GET", null, null, out bool _, out statusCode, headers: RequestHeaders);
                        response = JsonConvert.DeserializeObject<StateResponse>(responseContent);
                        if (response == null) continue;
                        errorCount = 0;
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        logger?.LogWarning($"Failed to request simulation state from ESSIM on {EssimManager.ApplicationUrl}.\r\n" +
                                                 $"HTTP status code: {statusCode}\r\n" +
                                                 $"Response content: {responseContent}\r\n" +
                                                 $"Exception: {e.Message}\r\n" +
                                                 $"{e.StackTrace}");

                        if (errorCount >= 3)
                        {
                            logger.LogError($"{errorCount} consecutive exceptions while monitoring simulation status. Exceptions will nog longer be accepted!");
                            throw;
                        }

                        continue;
                    }

                    simulationStateValue = response.Status;

                    switch (response.Status.ToUpper())
                    {
                        case "ERROR":
                            simulationStateDescription = response.Description;
                            logger?.LogError($"Error during simulation run {response.Description}");

                            lock (SyncRoot)
                            {
                                workingOnSimulation = false;
                                simulationFinishedHandler?.Invoke(null);
                            }
                            return;
                        case "RUNNING":
                            if (!string.IsNullOrEmpty(response.Description))
                                Double.TryParse(response.Description, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out simulationProgress);
                            simulationStateDescription = string.Empty;

                            break;
                        case "COMPLETE":
                            simulationProgress = 1.0;
                            simulationStateDescription = response.Description;
                            logger?.LogInformation($"Simulation {simulationId} finished");

                            lock (SyncRoot)
                            {
                                workingOnSimulation = false;
                                simulationQueueObject.EssimSimulationId = simulationId;
                                simulationFinishedHandler?.Invoke(simulationQueueObject);
                            }
                            return;
                        default:
                            logger?.LogWarning($"Essim engine reported unknown state: {response.Status}");
                            simulationStateDescription = response.Description;
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger?.LogError($"Error while monitoring simulation progress. {e.Message}\r\n{e.StackTrace}");
                }
            }
        }

        public static void Stop() => stopSimulation?.Set();
    }
}
