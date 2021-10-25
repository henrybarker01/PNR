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
using fn_bidtravel_pnrfinisher_portal.models;


using System.Collections;
using Newtonsoft.Json.Linq;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class AddRunHistoryEntity
    {
        [FunctionName("AddRunHistoryEntity")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("AddRunHistoryEntity() Triggered ...");


            string sStorageConnectionString = req.Headers["StorageConnectionString"]; //Read Storage Connection String
            string sJsonBody = await new StreamReader(req.Body).ReadToEndAsync(); //Read body of request
            string sTable = "tblRunHistory";



            //            string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";



            var oBodyJson = JObject.Parse(sJsonBody);
            string sRuleName = oBodyJson["RuleName"].ToString();
            string sNotes = oBodyJson["Notes"].ToString();
            string sLocator = oBodyJson["Locator"].ToString();
            string sStatus = oBodyJson["Status"].ToString();
            string sSegment = oBodyJson["Segment"].ToString();
            string sMandatory = oBodyJson["Mandatory"].ToString();


            //Connect to Storage Account
            CloudStorageAccount oStorageAccount = CloudStorageAccount.Parse(sStorageConnectionString);
            CloudTableClient oTableClient = oStorageAccount.CreateCloudTableClient();
            CloudTable oTable = oTableClient.GetTableReference(sTable);


            //Write Record
            string sResult = await InsertRunEntity(oTable, sLocator, sRuleName, sStatus, sNotes,sMandatory,sSegment);

            return new OkObjectResult(sResult);
        }





        public static async Task<string> InsertRunEntity(CloudTable table, string Locator,string RuleName,string Status, string Notes, string Mandatory, string Segment)
        {
            try
            {
                var oEntity = new RunHistoryItem();
                oEntity.PartitionKey = "RunHistory";
                oEntity.RowKey = oEntity.Id;
                oEntity.RuleName = RuleName;
                oEntity.Locator = Locator;
                oEntity.Status = Status;
                oEntity.Notes = Notes;
                oEntity.Segment = Segment;
                oEntity.Mandatory = Mandatory;
               // oEntity.DateTimeStamp = DateTime.Now;

                TableOperation oInsert = TableOperation.InsertOrMerge(oEntity);
                TableResult oResult = await table.ExecuteAsync(oInsert);
                Console.WriteLine(oResult.HttpStatusCode);
                return "Record added";
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
