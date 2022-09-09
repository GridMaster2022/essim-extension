using System;
using System.IO;
using System.Threading;
using essim_extension_core;
using essim_extension_core.Helpers;

namespace essim_extension_test_application
{
    class Program
    {
        private static string testContent = @"<?xml version='1.0' encoding='UTF-8'?>" +
                                             "<esdl:EnergySystem xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:esdl=\"http://www.tno.nl/esdl\" esdlVersion=\"v2102\" version=\"3\" id=\"ee10ca5f-a08d-4201-823c-fa7abcc1ba1b\" name=\"Untitled EnergySystem\" description=\"\">" +
                                             "  <energySystemInformation xsi:type=\"esdl:EnergySystemInformation\" id=\"751842b7-80e1-4e82-a222-fdebeca8a797\">" +
                                             "    <carriers xsi:type=\"esdl:Carriers\" id=\"0a3cb696-2558-48f4-9e23-aff11e1020a4\">" +
                                             "      <carrier xsi:type=\"esdl:HeatCommodity\" name=\"Heat\" id=\"fcd62e8f-ef7a-404c-8d3f-d94e9dd48cd3\" supplyTemperature=\"80.0\" returnTemperature=\"40.0\"/>" +
                                             "    </carriers>" +
                                             "    <quantityAndUnits xsi:type=\"esdl:QuantityAndUnits\" id=\"3f1a3603-2018-4a16-97e9-67eeee195a5b\">" +
                                             "      <quantityAndUnit xsi:type=\"esdl:QuantityAndUnitType\" physicalQuantity=\"ENERGY\" unit=\"JOULE\" multiplier=\"GIGA\" id=\"eb07bccb-203f-407e-af98-e687656a221d\" description=\"Energy in GJ\"/>" +
                                             "    </quantityAndUnits>" +
                                             "  </energySystemInformation>" +
                                             "  <instance xsi:type=\"esdl:Instance\" name=\"Untitled Instance\" id=\"42a64855-de86-4a66-b267-b00c66b43b58\">" +
                                             "    <area xsi:type=\"esdl:Area\" name=\"Untitled Area\" id=\"7bcf5cd9-d3fe-4626-859a-c16d89ddf552\">" +
                                             "      <asset xsi:type=\"esdl:GeothermalSource\" name=\"GeothermalSource_b572\" id=\"b572d6cb-313e-489f-b489-a86bbd6ecd21\">" +
                                             "        <geometry xsi:type=\"esdl:Point\" lon=\"4.702792167663575\" CRS=\"WGS84\" lat=\"52.12170613337859\"/>" +
                                             "        <port xsi:type=\"esdl:OutPort\" id=\"15ff7099-8b7f-46af-bcc4-63e03f17722e\" name=\"Out\" connectedTo=\"a09857b5-5093-499c-83ce-54ecc54a7518\" carrier=\"fcd62e8f-ef7a-404c-8d3f-d94e9dd48cd3\">" +
                                             "          <profile xsi:type=\"esdl:SingleValue\" value=\"5.0\" id=\"a3804b62-4205-4afb-bf78-60cd73d339f2\">" +
                                             "            <profileQuantityAndUnit xsi:type=\"esdl:QuantityAndUnitReference\" reference=\"eb07bccb-203f-407e-af98-e687656a221d\"/>" +
                                             "          </profile>" +
                                             "        </port>" +
                                             "      </asset>" +
                                             "      <asset xsi:type=\"esdl:HeatingDemand\" name=\"HeatingDemand_b505\" id=\"b505c10b-bde4-4606-8d68-7f8e0188c383\">" +
                                             "        <geometry xsi:type=\"esdl:Point\" lon=\"4.712383747100831\" CRS=\"WGS84\" lat=\"52.12191692831886\"/>" +
                                             "        <port xsi:type=\"esdl:InPort\" connectedTo=\"15ff7099-8b7f-46af-bcc4-63e03f17722e\" id=\"a09857b5-5093-499c-83ce-54ecc54a7518\" name=\"In\" carrier=\"fcd62e8f-ef7a-404c-8d3f-d94e9dd48cd3\">" +
                                             "          <profile xsi:type=\"esdl:SingleValue\" value=\"5.0\" id=\"e19ae2d6-3ff7-494c-b8bf-86450d855838\">" +
                                             "            <profileQuantityAndUnit xsi:type=\"esdl:QuantityAndUnitReference\" reference=\"eb07bccb-203f-407e-af98-e687656a221d\"/>" +
                                             "          </profile>" +
                                             "        </port>" +
                                             "      </asset>" +
                                             "    </area>" +
                                             "  </instance>" +
                                             "</esdl:EnergySystem>";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Environment.SetEnvironmentVariable("HTTP_SERVER_SCHEME", "http");
            Environment.SetEnvironmentVariable("HTTP_SERVER_PORT", "8112");
            Environment.SetEnvironmentVariable("HTTP_SERVER_PATH", "essim");
            Environment.SetEnvironmentVariable("INFLUXDB_INTERNAL_URL", "http://influxdb-stripped:8086");
            Environment.SetEnvironmentVariable("INFLUXDB_EXTERNAL_URL", "http://influxdb-stripped:8086");

            //InfluxDbClient.Go("60d58fef39ab973001d71bb4");
            //while (true)
            //{
            //    Thread.Sleep(300);
            //}
            //EsdlSimulationTest();
            SqsClientTest();
        }

        private static void EsdlSimulationTest()
        {
            string esdlContent = File.ReadAllText(@"PATH_TO_ESDL_PLACEHOLDER");
            //string esdlContent = testContent;
            SimulationProcessor.ProcessEsdlContent(esdlContent, null);

            while (true)
            {
                Thread.Sleep(300);

                string id = SimulationProcessor.SimulationId;
                string url = SimulationProcessor.SimulationDashboardUrl;
                double progress = SimulationProcessor.SimulationProgress;
                string state = SimulationProcessor.SimulationStateValue;
                string description = SimulationProcessor.SimulationStateDescription;

                int i = 0;

                //if (SimulationProcessor.SimulationStateValue == "COMPLETE") break;
            }
        }

        private static void SqsClientTest()
        {
            Environment.SetEnvironmentVariable("AWS_ESSIM_QUEUE_URL", "https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_ID}/gridmaster_essim_queue");
            Environment.SetEnvironmentVariable("AWS_ESSIM_EXPORT_QUEUE_URL", "https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_ID}/gridmaster_essim_export_queue");

            AwsSqsClient.ReadMessageFromSqs(SimulationProcessor.ProcessEsdlContent, null, null);

            while (true)
            {
                Thread.Sleep(300);
                if (SimulationProcessor.SimulationStateValue == "COMPLETE") break;
            }
        }
    }
}
