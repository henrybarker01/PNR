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
using System.Collections.Generic;

namespace fn_bidtravel_pnrfinisher_getsabreauthtoken
{
    public static class GetSabreToken
    {
        [FunctionName("GetSabreToken")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("Attempting to get Sabre Token ...");

            try
            {
                string sSabreUserID = req.Headers["SabreUserID"];
                string sURL = req.Headers["TokenURL"];

                log.LogInformation("Supplied UserID = " + sSabreUserID);
                log.LogInformation("Supplied URL = " + sURL);


                if(string.IsNullOrEmpty(sSabreUserID) == false || string.IsNullOrEmpty(sURL))
                {
                    //ServiceRequest oSoapCall = new ServiceRequest();
                    ////Add Header Items
                    //Dictionary<string, string> oHeader = new Dictionary<string, string>();
                    //oHeader.Add("Authorization", "Basic " + sSabreUserID);
                    //oHeader.Add("Content-Type", "application/x-www-form-urlencoded");
                    //oHeader.Add("grant_type", "client_credentials");

                    ////oSoapCall.CreateSOAPWebRequest(oHeader, SOAP.MethodType.POST);

                    //var oWebservice_Result = oSoapCall.InvokeRESTService(sURL, oHeader, ServiceRequest.MethodType.POST);

                    var oWebservice_Result = BidTravel_Common.Sabre.GetSabreToken(sSabreUserID, sURL);


                    oReturn.Content = oWebservice_Result;

                    log.LogInformation("WebCall Results = " + oReturn.Content);

                }
                else
                {
                    log.LogInformation("Please supply a SabreUserID and TokenURL in the header of the message...");
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
