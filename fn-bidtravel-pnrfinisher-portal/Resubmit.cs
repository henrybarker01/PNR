using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class Resubmit
    {
        public struct ResubmitResponse
        {
            public bool Successful { get; set; }
            public string Result { get; set; }

        }

        [FunctionName("Resubmit")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            ResubmitResponse oResponse = new ResubmitResponse();

            try
            {
                log.LogInformation("Resubmit() Triggered ...");

                

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if(data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        string sLocator = item.locator;
                    }

                    oResponse.Result = "Re-submitted";
                    oResponse.Successful = true;
                    oReturn.Content = JsonConvert.SerializeObject(oResponse);


                }
                else
                {
                    oResponse.Result = "No locator's suppled";
                    oResponse.Successful = false;
                    oReturn.Content = JsonConvert.SerializeObject(oResponse);

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
