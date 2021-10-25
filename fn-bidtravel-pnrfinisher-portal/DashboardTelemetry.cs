using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class DashboardTelemetry
    {
        private struct DashboardTelemeteryItem
        {
            public string Item { get; set; }
            public int Value { get; set; }
        }



        [FunctionName("GetDashboardTelemetry")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();



            log.LogInformation("GetDashboardTelemetry() Invoked ...");

            try
            {
                //Get Parameters

                string sPCC = req.Query["PCC"];
                string sDateTimeStart = req.Query["StartDate"];
                string sDateTimeEnd = req.Query["EndDate"];

                string sReturnPayload = Newtonsoft.Json.JsonConvert.SerializeObject(GetDashboardTelemetry());





                //var connectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";
                //var storageAccount = CloudStorageAccount.Parse(connectionString);
                //var tableClient = storageAccount.CreateCloudTableClient();
                //CloudTable table = tableClient.GetTableReference("tblOperations");
                //await table.CreateIfNotExistsAsync();

                //var result = await GetEntitiesFromTable(table);

                oReturn = new ContentResult { Content = sReturnPayload, ContentType = "application/json", StatusCode = 200 };

            }
            catch (Exception ex)
            {

                oReturn = new ContentResult { Content = ex.Message, ContentType = "application/xml", StatusCode = 500 };
            }


            return oReturn;
        }




    



        private static async Task<IEnumerable<T>> GetEntitiesFromTable<T>(CloudTable table) where T : ITableEntity, new()
        {
            TableQuerySegment<T> querySegment = null;
            var entities = new List<T>();
            var query = new TableQuery<T>();

            do
            {
                querySegment = await table.ExecuteQuerySegmentedAsync(query, querySegment?.ContinuationToken);
                entities.AddRange(querySegment.Results);
            } while (querySegment.ContinuationToken != null);

            return entities;
        }



        private static List<DashboardTelemeteryItem> GetDashboardTelemetry()
        {
            List<DashboardTelemeteryItem> oReturn = new List<DashboardTelemeteryItem>();

            try
            {
                DashboardTelemeteryItem oTotalPNRs = new DashboardTelemeteryItem();
                oTotalPNRs.Item = "PNRs Processed";
                oTotalPNRs.Value = 23;
                oReturn.Add(oTotalPNRs);

                DashboardTelemeteryItem oRulesApplied_Applied = new DashboardTelemeteryItem();
                oRulesApplied_Applied.Item = "RulesApplied_Applied";
                oRulesApplied_Applied.Value = 97;
                oReturn.Add(oRulesApplied_Applied);

                DashboardTelemeteryItem oRulesApplied_NotApplied = new DashboardTelemeteryItem();
                oRulesApplied_NotApplied.Item = "RulesApplied_NotApplied";
                oRulesApplied_NotApplied.Value = 3;
                oReturn.Add(oRulesApplied_NotApplied);

                DashboardTelemeteryItem oFailed = new DashboardTelemeteryItem();
                oFailed.Item = "Failed";
                oFailed.Value = 1;
                oReturn.Add(oFailed);

                DashboardTelemeteryItem oRetries = new DashboardTelemeteryItem();
                oRetries.Item = "Retries";
                oRetries.Value = 4;
                oReturn.Add(oRetries);
            }
            catch (Exception ex)
            {

                throw;
            }



            return oReturn;
        }


    }
}
