using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fn_bidtravel_pnrfinisher_pnrhandler_dev
{
    public static class UpdateRemark
    {
        [FunctionName("UpdateRemark")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            //Get Parameters
            string sLocator = req.Query["Locator"]; //Optionally supplied for API call purposes
            if (String.IsNullOrEmpty(sLocator)) sLocator = req.Headers["Locator"];

            sLocator = sLocator.Replace(".xml", string.Empty);
            log.LogInformation("LocatorID = " + sLocator);














            return oReturn ;
        }
    }
}

