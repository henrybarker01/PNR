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
using Microsoft.WindowsAzure.Storage.Table;
using fn_bidtravel_pnrfinisher_portal.models;
using Microsoft.WindowsAzure.Storage;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class ChangeUser
    {
        public struct ChangeUserRoleResponse
        {
            public bool Successful { get; set; }
            public string Result { get; set; }

        }

        [FunctionName("ChangeUser")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string sUniqueID = data?.uniqueid;
                string sUniqueIDtoChange = data?.uniqueidtochange;
                string sRole = data?.role;
                //string sUsername = data?.username;
                bool bActive = false;

                bool bChangeRole = false;
                bool bChangeActive = false;

                //Detrurmine what to change
                if (sRole != null)
                    bChangeRole = true;

                if (data.active == null)
                    bActive = false;
                else
                {
                    bChangeActive = true;
                    bActive = data?.active;
                }


                ChangeUserRoleResponse oResponse = new ChangeUserRoleResponse();

                if (sUniqueID == null || sUniqueIDtoChange == null)
                {
                    oResponse.Result = "Invalid Parameters";
                    oResponse.Successful = false;
                }
                else
                {
                    string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";//req.Headers["StorageConnectionString"]; //Read Storage Connection String

                    //Connect to Storage Account
                    CloudStorageAccount oStorageAccount = CloudStorageAccount.Parse(sStorageConnectionString);
                    CloudTableClient oTableClient = oStorageAccount.CreateCloudTableClient();
                    CloudTable oTable = oTableClient.GetTableReference("tblUsers");

                    List<string> oColumns = new List<string>();
                    oColumns.Add("Role");
                    oColumns.Add("Active");


                    //Check if current user is active and has admin role
                    TableOperation oRetrieve = TableOperation.Retrieve<UserItem>("Users", sUniqueID.ToLower(), oColumns);
                    TableResult oResult = await oTable.ExecuteAsync(oRetrieve);
                    var oUser = (UserItem)oResult.Result;
                    if (oUser != null)
                    {
                        if (oUser.Role.ToLower() == "admin" && oUser.Active == true)
                        {
                            //Check if user already exists?
                            oRetrieve = TableOperation.Retrieve<UserItem>("Users", sUniqueIDtoChange.ToLower(), oColumns);
                            oResult = await oTable.ExecuteAsync(oRetrieve);
                            oUser = (UserItem)oResult.Result;

                            if (oUser != null)
                            {
                                UserItem oNewUserRecord = new UserItem();
                                oNewUserRecord.Active = true;
                                oNewUserRecord.PartitionKey = "Users";
                                oNewUserRecord.RowKey = sUniqueIDtoChange.ToLower();
                                //oNewUserRecord.Username = sUsername;
                                //oNewUserRecord.UniqueID = sUniqueIDtoChange;

                                if (bChangeRole == true)
                                    oNewUserRecord.Role = sRole;

                                if (bChangeActive == true)
                                    oNewUserRecord.Active = bActive;

                                TableOperation oInsert = TableOperation.InsertOrMerge(oNewUserRecord);
                                TableResult oInsertResult = await oTable.ExecuteAsync(oInsert);

                                if (oInsertResult.HttpStatusCode == 200 || oInsertResult.HttpStatusCode == 204)
                                {
                                    oResponse.Result = "User successfully changed";
                                    oResponse.Successful = true;
                                }
                            }
                            else
                            {
                                oResponse.Result = "User does not exist";
                                oResponse.Successful = false;
                            }
                        }
                        else
                        {
                            oResponse.Result = "Unauthorised to perform this operation";
                            oResponse.Successful = false;
                        }
                    }
                    else
                    {
                        oResponse.Result = "Unauthorised to perform this operation";
                        oResponse.Successful = false;
                    }
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
