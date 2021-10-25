using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using fn_bidtravel_pnrfinisher_portal.models;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class GetProcessedItem
    {
        [FunctionName("GetProcessedItem")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("GetProcessedItem() Triggered ...");


            try
            {
                string sLocator = req.Query["Locator"];

                ProcessedItem oProcessedItem = new ProcessedItem();
                oProcessedItem.DateTimeStamp = DateTime.Now.AddDays(-3);
                oProcessedItem.Identifier = sLocator;
                oProcessedItem.PCC = "7SVG";
                oProcessedItem.Reason = "Successful";
                oProcessedItem.Retries = 0;
                oProcessedItem.Rules = 4;
                oProcessedItem.Status = "Passed";



                string sReturnPayload = Newtonsoft.Json.JsonConvert.SerializeObject(oProcessedItem);

                oReturn = new ContentResult { Content = sReturnPayload, ContentType = "application/json", StatusCode = 200 };


            }
            catch (Exception)
            {
                oReturn = new ContentResult { StatusCode = 500 };
            }

            return oReturn;
        }
    }
}
