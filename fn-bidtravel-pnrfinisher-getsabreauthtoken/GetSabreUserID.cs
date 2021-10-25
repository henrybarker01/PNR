using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BidTravel_Common;
using System.Text;

namespace fn_bidtravel_pnrfinisher_getsabreauthtoken
{
    public static class GetSabreUserID
    {
        public struct ReturnPayload { public string UserID { get; set; } }


        [FunctionName("GetSabreUserID")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            ReturnPayload oReturnPayload = new ReturnPayload();

            log.LogInformation("Attempting to generate Sabre UserID ...");

            try
            {
                string sUsername = req.Headers["Username"]; //Read Token from header (easier as the url encoding is taken care of)
                string sPassword = req.Headers["Password"];
                string sPCC = req.Headers["PCC"];

                log.LogInformation("Username = " + sUsername);
                log.LogInformation("Password = " + sPassword);
                log.LogInformation("PCC = " + sPCC);

                oReturnPayload.UserID = BidTravel_Common.Utilities.GenerateSabreTokenCredentials(sUsername, sPassword, sPCC);


                oReturn.Content = JsonConvert.SerializeObject(oReturnPayload);

                log.LogInformation("Generated UserID = " + oReturnPayload.UserID);

            }
            catch (Exception ex)
            {
            }



            return oReturn;
        }












    }
}
