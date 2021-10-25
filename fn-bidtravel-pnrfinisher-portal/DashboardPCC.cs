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
    public static class DashboardPCC
    {
        private struct PCCItem
        {
            public string PCC { get; set; }
        }




        [FunctionName("GetPCCList")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("GetPCCList() Invoked ...");
            try
            {
                string sReturnPayload = Newtonsoft.Json.JsonConvert.SerializeObject(GetPCCList());

                oReturn = new ContentResult { Content = sReturnPayload, ContentType = "application/json", StatusCode = 200 };

            }
            catch (Exception ex)
            {

                oReturn = new ContentResult { Content = ex.Message, ContentType = "application/xml", StatusCode = 500 };
            }

            return oReturn;
        }





        private static List<PCCItem> GetPCCList()
        {
            List<PCCItem> oReturn = new List<PCCItem>();

            try
            {
                PCCItem oItem1 = new PCCItem();
                oItem1.PCC = "7SVG";
                oReturn.Add(oItem1);

                PCCItem oItem2 = new PCCItem();
                oItem2.PCC = "AG7KL";
                oReturn.Add(oItem2);

                PCCItem oItem3 = new PCCItem();
                oItem3.PCC = "HF5WE";
                oReturn.Add(oItem3);
            }
            catch (Exception ex)
            {

                throw;
            }



            return oReturn;

        }
    }
}
