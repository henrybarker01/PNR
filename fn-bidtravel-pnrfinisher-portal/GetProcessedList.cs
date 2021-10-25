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
using fn_bidtravel_pnrfinisher_portal.models;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class GetProcessedList
    {
        //public struct ProcessedItem
        //{
        //    public string Identifier { get; set; }
        //    public string PCC { get; set; }
        //    public DateTime DateTimeStamp { get; set; }
        //    public int Rules { get; set; }
        //    public int Retries { get; set; }
        //    public string Reason { get; set; }
        //    public string Status { get; set; }
        //}


        [FunctionName("GetProcessedList")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("GetProcessedList() Triggered ...");

            try
            {
                string sPCC = req.Query["PCC"];
                string sDateTimeStart = req.Query["StartDate"];
                string sDateTimeEnd = req.Query["EndDate"];

                string sReturnPayload = Newtonsoft.Json.JsonConvert.SerializeObject(GetProcessedItems());

                oReturn = new ContentResult { Content = sReturnPayload, ContentType = "application/json", StatusCode = 200 };

            }
            catch (Exception)
            {
                oReturn = new ContentResult {  StatusCode = 500 };
            }

            return oReturn;
        }


        public static List<ProcessedItem> GetProcessedItems()
        {
            List<ProcessedItem> oReturn = new List<ProcessedItem>();


            try
            {
                ProcessedItem oItem = new ProcessedItem();

                Random oRnd = new Random();

                oItem.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem.Identifier = "CRWEXI";
                oItem.PCC = "7SVG";
                oItem.Rules = 4;
                oItem.Retries = 0;
                oItem.Reason = "Successful";
                oItem.Status = "Passed";

                oReturn.Add(oItem);


                ProcessedItem oItem2 = new ProcessedItem();
                oItem2.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem2.Identifier = "LFFTPJ";
                oItem2.PCC = "7SVG";
                oItem2.Rules = 8;
                oItem2.Retries = 0;
                oItem2.Reason = "Successful";
                oItem2.Status = "Passed";

                oReturn.Add(oItem2);

                ProcessedItem oItem3 = new ProcessedItem();
                oItem3.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem3.Identifier = "LFFTPJ";
                oItem3.PCC = "P9DF";
                oItem3.Rules = 6;
                oItem3.Retries = 0;
                oItem3.Reason = "Unable to submit to SABRE";
                oItem3.Status = "Failed";

                oReturn.Add(oItem3);


                ProcessedItem oItem4 = new ProcessedItem();
                oItem4.DateTimeStamp = DateTime.Now.AddSeconds(oRnd.Next(1, 100));

                oItem4.Identifier = "CRWEXI";
                oItem4.PCC = "P9DF";
                oItem4.Rules = 6;
                oItem4.Retries = 0;
                oItem4.Reason = "Successful";
                oItem4.Status = "Passed";

                oReturn.Add(oItem4);


            }
            catch (Exception)
            {

                throw;
            }

            return oReturn;
        }
    }
}
