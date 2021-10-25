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
using Azure.Storage.Blobs;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class GetOriginalPNR
    {
        [FunctionName("GetOriginalPNR")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("GetOriginalPNR Triggered ...");

            try
            {
                string sLocator = req.Query["Locator"];
                string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";//req.Headers["StorageConnectionString"]; //Read Storage Connection String

                //Connect to Storage Account
                BlobServiceClient blobServiceClient = new BlobServiceClient(sStorageConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("pnrincoming");
                BlobClient blobClient = containerClient.GetBlobClient(sLocator + ".xml");
                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadAsync();
                    using (var streamReader = new StreamReader(response.Value.Content))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            var sFileContent = await streamReader.ReadToEndAsync();//.ReadLineAsync();
                            oReturn.Content = sFileContent;
                            oReturn.StatusCode = 200;
                        }
                    }
                }
                else
                {
                    oReturn.Content = "No record of locator : " + sLocator;
                    oReturn.StatusCode = 500;

                }

            }
            catch (Exception ex)
            {
                throw;
            }



            return oReturn ;
        }
    }
}
