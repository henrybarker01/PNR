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
    public static class GetUsers
    {
        public struct GetUsersResponse
        {
            public bool Successful { get; set; }
            public string Result { get; set; }

        }


        [FunctionName("GetUsers")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("GetUsers() Triggered ...");

            string sUniqueID = req.Query["UniqueID"];

            try
            {
                List<UserItem> oListofUsers = new List<UserItem>();

                string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";//req.Headers["StorageConnectionString"]; //Read Storage Connection String

                GetUsersResponse oResponse = new GetUsersResponse();

                //Connect to Storage Account
                CloudStorageAccount oStorageAccount = CloudStorageAccount.Parse(sStorageConnectionString);
                CloudTableClient oTableClient = oStorageAccount.CreateCloudTableClient();
                CloudTable oTable = oTableClient.GetTableReference("tblUsers");

                if (sUniqueID == null || sUniqueID == null)
                {
                    oResponse.Result = "Invalid Parameters";
                    oResponse.Successful = false;
                    oReturn.Content = JsonConvert.SerializeObject(oResponse);
                }
                else
                {
                    List<string> oColumns = new List<string>();
                    oColumns.Add("Role");
                    oColumns.Add("Active");
                    oColumns.Add("UniqueID");
                    oColumns.Add("Username");

                    //Check if current user is active and has admin role
                    TableOperation oRetrieve = TableOperation.Retrieve<UserItem>("Users", sUniqueID.ToLower(), oColumns);
                    TableResult oResult = await oTable.ExecuteAsync(oRetrieve);
                    var oUser = (UserItem)oResult.Result;
                    if (oUser != null)
                    {
                        if (oUser.Role.ToLower() == "admin" && oUser.Active == true)
                        {
                            TableQuery<UserItem> oQuery = new TableQuery<UserItem>();
                            TableContinuationToken token = null;
                            var queryResult = oTable.ExecuteQuerySegmentedAsync(new TableQuery<UserItem>(), token);

                            oListofUsers.AddRange(queryResult.Result);

                            oReturn.Content = JsonConvert.SerializeObject(oListofUsers); 
                        }

                        else
                        {
                            oResponse.Result = "Unauthorised to perform this operation";
                            oResponse.Successful = false;
                            oReturn.Content = JsonConvert.SerializeObject(oResponse);
                        }
                    }
                    else
                    {
                        oResponse.Result = "Unauthorised to perform this operation";
                        oResponse.Successful = false;
                        oReturn.Content = JsonConvert.SerializeObject(oResponse);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }


            return oReturn;
        }
    }
}
