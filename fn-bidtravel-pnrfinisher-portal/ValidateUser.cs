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
using fn_bidtravel_pnrfinisher_portal.models;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class ValidateUser
    {
        public struct ValidationResponse 
        {
            public string UniqueID { get; set; }
            public string Role { get; set; }
            public bool Active { get; set; }
            public bool Validated { get; set; }
        }


        [FunctionName("ValidateUser")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("ValidateUser() Triggered ...");

            string sUniqueID = req.Query["UniqueID"];

            try
            {
                string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";//req.Headers["StorageConnectionString"]; //Read Storage Connection String


                //Connect to Storage Account
                CloudStorageAccount oStorageAccount = CloudStorageAccount.Parse(sStorageConnectionString);
                CloudTableClient oTableClient = oStorageAccount.CreateCloudTableClient();
                CloudTable oTable = oTableClient.GetTableReference("tblUsers");

                List<string> oColumns = new List<string>();
                oColumns.Add("Role");
                oColumns.Add("Active");
                //oColumns.Add("Validated");

                TableOperation oRetrieve = TableOperation.Retrieve<UserItem>("Users", sUniqueID.ToLower(), oColumns);
                TableResult oResult = await oTable.ExecuteAsync(oRetrieve);
                var oUser = (UserItem)oResult.Result;

                ValidationResponse oResponse = new ValidationResponse();

                if (oUser != null)
                {
                    oResponse.UniqueID = sUniqueID;
                    oResponse.Active = oUser.Active;
                    oResponse.Role = oUser.Role;
                    oResponse.Validated = true;
                }
                else
                {
                    oResponse.Active = false ;
                    oResponse.Role = "None";
                    oResponse.Validated = false;
                }
                oReturn.Content = JsonConvert.SerializeObject(oResponse);

            }
            catch (Exception)
            {

                throw;
            }


            return oReturn;
        }
    }
}
