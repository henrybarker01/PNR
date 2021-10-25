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
    public static class MovePNR
    {
        [FunctionName("MovePNR")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            bool bErrorEncountered = false; //Error Flag

            string sToken = req.Headers["Token"]; //Read Token from header (easier as the url encoding is taken care of)
            string sLocator = req.Query["Locator"]; //Optionally supplied for API call purposes
            string sQueueToMoveTo = req.Query["Queue"]; 
            if (String.IsNullOrEmpty(sLocator)) sLocator = req.Headers["Locator"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync(); //Read body of request

            //Check parameters are supplied
            if ((string.IsNullOrEmpty(sQueueToMoveTo) && string.IsNullOrEmpty(sToken)) || string.IsNullOrEmpty(sLocator))
            {
                log.LogInformation("ERROR ! Parameters not supplied (Require Token and Locator and QueueToMoveTo)");
                oReturn = new ContentResult { Content = "Parameters not supplied (Require Token and Locator and QueueToMoveTo)", ContentType = "text/html; charset=UTF-8", StatusCode = 500 };
            }
            else
            {
                //oReturn = new ContentResult { Content = ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 200 };




                //Add Located to Header (Used by Logic App for identification)
                req.HttpContext.Response.Headers.Add("Locator", sLocator);

                //Indicate if any errors were detected (Used by Logic App for easier error detection)
                req.HttpContext.Response.Headers.Add("Errors", bErrorEncountered.ToString());

            }

            return oReturn;
        }
    }
}

