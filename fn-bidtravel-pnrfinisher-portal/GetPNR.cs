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
using System.Collections.Generic;
using fn_bidtravel_pnrfinisher_portal.models;
using Newtonsoft.Json.Linq;
using BidTravel_Common;
using System.Net;
using System.Xml;

namespace fn_bidtravel_pnrfinisher_portal
{
    public static class GetPNR
    {
        [FunctionName("GetPNR")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            log.LogInformation("Triggered GetPNR");

            try
            {
                string sLocator = req.Query["Locator"];
                string sStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storagepnrfinisherdev;AccountKey=2T/vNkrlrQo4mDVqq/eMJz3vdra8VmBKao2qANRfCrrspmUj8cSHTqnIYZosvlLmPOvePh5eJJAU4d7RBg46EA==;EndpointSuffix=core.windows.net";//req.Headers["StorageConnectionString"]; //Read Storage Connection String

                //Connect to Storage Account
                CloudStorageAccount oStorageAccount = CloudStorageAccount.Parse(sStorageConnectionString);
                CloudTableClient oTableClient = oStorageAccount.CreateCloudTableClient();
                CloudTable oTable = oTableClient.GetTableReference("Config");


                #region Get Sabre UserID

                List<string> oColumns = new List<string>();
                oColumns.Add("Value");

                TableOperation oRetrieve = TableOperation.Retrieve<SettingItem>("Settings", "Sabre_PCC", oColumns);
                TableResult oResult = await oTable.ExecuteAsync(oRetrieve);
                var oSetting = (SettingItem)oResult.Result;
                string sSabrePCC = oSetting.Value;

                oRetrieve = TableOperation.Retrieve<SettingItem>("Settings", "Sabre_Username", oColumns);
                oResult = await oTable.ExecuteAsync(oRetrieve);
                oSetting = (SettingItem)oResult.Result;
                string sSabreUsername = oSetting.Value;

                oRetrieve = TableOperation.Retrieve<SettingItem>("Settings", "Sabre_Password", oColumns);
                oResult = await oTable.ExecuteAsync(oRetrieve);
                oSetting = (SettingItem)oResult.Result;
                string sSabrePassword = oSetting.Value;

                oRetrieve = TableOperation.Retrieve<SettingItem>("Settings", "BaseURL", oColumns);
                oResult = await oTable.ExecuteAsync(oRetrieve);
                oSetting = (SettingItem)oResult.Result;
                string sSabreBaseURL = oSetting.Value;


                string sUserID = BidTravel_Common.Utilities.GenerateSabreTokenCredentials(sSabreUsername, sSabrePassword, sSabrePCC);

                string sSabreToken = BidTravel_Common.Sabre.GetSabreToken(sUserID, sSabreBaseURL + "/v2/auth/token");

                if (sSabreToken.Length > 0)
                {
                    //Extract Token
                    var oJson = JObject.Parse(sSabreToken);
                    sSabreToken = oJson["access_token"].ToString();

                }


                #endregion


                var oWebservice_Result = InvokeService(sLocator, sSabreToken);


                //Validate Response is a PNR
                {
                    //TODO : Since xml namespaces are used in their documents - you need to cater for 

                    //Check for error nodes in XML
                    if (oWebservice_Result.GetElementsByTagName("stl19:Errors").Count > 0 || oWebservice_Result.GetElementsByTagName("soap-env:Fault").Count > 0) //Issues encountered
                    {
                        log.LogInformation("WARNING ! Server response contains errors. Check Error folder on storage for details ...");
                        //log.LogInformation(ConvertXMLtoString(oWebservice_Result));
                        // bErrorEncountered = true;
                        oReturn = new ContentResult { Content = Utilities.ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 500 };
                    }
                    else //valid PNR
                    {
                        oReturn = new ContentResult { Content = Utilities.ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 200 };
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }


            return oReturn;
        }





        public static HttpWebRequest CreateSOAPWebRequest()
        {
            //Making Web Request    
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(@"https://sws-crt.cert.havail.sabre.com");


            try
            {
                //SOAPAction we want to call    
                oWebRequest.Headers.Add(@"SOAPAction:getReservationRQ");

                //Content_type    
                oWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
                oWebRequest.Accept = "text/xml";

                //HTTP method    
                oWebRequest.Method = "POST";
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return oWebRequest;
        }


        public static XmlDocument InvokeService(string Locator, string Token)
        {
            XmlDocument oReturn = new XmlDocument();

            WebResponse Serviceres = null;
            try
            {
                //Call CreateSOAPWebRequest method    
                HttpWebRequest request = CreateSOAPWebRequest();

                XmlDocument SOAPReqBody = new XmlDocument();

                var binDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));

                //var path = System.IO.Path.Combine(context.FunctionDirectory, "Sabre_GetPNR_Payload.xml");

                //Read XML Payload from File
                string sFilename = rootDirectory + @"\Sabre_GetPNR_Payload.xml";

                //Replace placeholders from payload Template
                string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Locator}}", Locator).Replace("{{Token}}", Token);


                //SOAPReqBody.Load(sFilename);
                SOAPReqBody.LoadXml(sTemplateFileText);
                using (Stream stream = request.GetRequestStream())
                {
                    SOAPReqBody.Save(stream);
                }
                //Geting response from request    
                try
                {
                    using (Serviceres = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                        {
                            //reading stream    
                            string ServiceResult = rd.ReadToEnd();
                            //writting stream result on console    

                            oReturn.LoadXml(ServiceResult);

                            //Console.WriteLine(ServiceResult);
                            //Console.ReadLine();
                        }
                    }
                }
                catch (WebException ex)
                {
                    string oExceptionMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                    oReturn.LoadXml(oExceptionMessage);
                    //log.LogInformation("ERROR ! - unable to call sabre web service");
                }
            }
            catch (Exception ex)
            {
                //log.LogInformation(ex.Message);
                throw;
            }

            return oReturn;
        }




    }
}
