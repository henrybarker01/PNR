using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fn_bidtravel_pnrfinisher_pnrhandler_dev
{
    public static class QueryFOD
    {

        static HttpClient client = new HttpClient();

        [FunctionName("QueryFOD")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");



            // HttpResponseMessage response = await client.GetAsync("https://apim-bidtravel-dev.azure-api.net/ENS/PNR/Queue");

            //XmlDocument oResponse = InvokeService("http://bidtravel.traveport.services/ITravelPortService/GetLocale", "REN", "CPT");


            BidTravelFOD oFOD = new BidTravelFOD("http://10.10.40.42/TravelPortService2/TravelPortService.svc");

            //  string ss = oFOD.GetLocale("CPT", "REN");

            //string sGroupOrBrand = Utilities.GetXMLValue(oDoc, "or114:Group").ToUpper(); ;




            string responseMessage = "";// ConvertXMLtoString(oResponse);// response.Content.ToString();

            return new OkObjectResult(responseMessage);
        }



        public static HttpWebRequest CreateSOAPWebRequest(string SOAPAction)
        {
            //Making Web Request    
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(@"http://10.10.40.42/TravelPortService2/TravelPortService.svc");


            try
            {
                //SOAPAction we want to call    
                oWebRequest.Headers.Add(@"SOAPAction:" + SOAPAction);

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


        public static XmlDocument InvokeService(string SOAPAction, string Group, string Destination)
        {
            XmlDocument oReturn = new XmlDocument();

            WebResponse Serviceres = null;
            try
            {
                //Call CreateSOAPWebRequest method    
                HttpWebRequest request = CreateSOAPWebRequest(SOAPAction);

                XmlDocument SOAPReqBody = new XmlDocument();

                var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));


                //Read XML Payload from File
                string sFilename = rootDirectory + @"\FOD_GetLocale_Payload.xml";

                //Replace placeholders from payload Template
                string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Group}}", Group).Replace("{{Destination}}", Destination);


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






        public static string ConvertXMLtoString(XmlDocument xml)
        {
            string sReturn = string.Empty;

            try
            {
                StringWriter oStringWriter = new StringWriter();
                XmlTextWriter oXMLTextWriter = new XmlTextWriter(oStringWriter);
                xml.WriteTo(oXMLTextWriter);

                sReturn = oStringWriter.ToString();
            }
            catch (Exception)
            {

                throw;
            }

            return sReturn;

        }




    }
}

