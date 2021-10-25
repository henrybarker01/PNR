using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class DashboardMostRecent
    {
        private struct DashboardMostRecentItem
        {
            public string Identifier { get; set; }
            public string PCC { get; set; }
            public DateTime DateTimeStamp { get; set; }
            public int Rules { get; set; }
            public string Status { get; set; }
        }


        [FunctionName("GetDashboardMostRecent")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();



            log.LogInformation("GetDashboardMostRecent() Invoked ...");

            try
            {
                //Get Parameters

                string sPCC = req.Query["PCC"];
                string sDateTimeStart = req.Query["StartDate"];
                string sDateTimeEnd = req.Query["EndDate"];

                string sReturnPayload = Newtonsoft.Json.JsonConvert.SerializeObject(GetDashboardMostRecent());





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



        private static List<DashboardMostRecentItem> GetDashboardMostRecent()
        {
            List<DashboardMostRecentItem> oReturn = new List<DashboardMostRecentItem>();


            try
            {
                DashboardMostRecentItem oItem = new DashboardMostRecentItem();

                Random oRnd = new Random();

                oItem.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem.Identifier = "FVHUQSRS";
                oItem.PCC = "7SVG";
                oItem.Rules = 4;
                oItem.Status = "Passed";

                oReturn.Add(oItem);
                

                oItem.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem.Identifier = "CDTRWKAA";
                oItem.PCC = "7SVG";
                oItem.Rules = 8;
                oItem.Status = "Passed";

                oReturn.Add(oItem);


                oItem.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem.Identifier = "GDUYUESC";
                oItem.PCC = "P9DF";
                oItem.Rules = 6;
                oItem.Status = "Failed";

                oReturn.Add(oItem);

                oItem.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem.Identifier = "GDCCETDS";
                oItem.PCC = "P9DF";
                oItem.Rules = 6;
                oItem.Status = "Passed";

                oReturn.Add(oItem);






            }
            catch (Exception)
            {

                throw;
            }

            return oReturn;
        }
    }


}
