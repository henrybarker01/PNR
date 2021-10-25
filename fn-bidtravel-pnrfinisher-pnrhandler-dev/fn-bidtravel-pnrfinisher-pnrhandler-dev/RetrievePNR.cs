using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace fn_bidtravel_pnrfinisher_pnrhandler_dev
{
    public static class RetrievePNR
    {
        [FunctionName("RetrievePNR")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            bool bErrorEncountered = false; //Error Flag

            try
            {
                string sToken = req.Headers["Token"]; //Read Token from header (easier as the url encoding is taken care of)
                string sLocator = req.Query["Locator"]; //Optionally supplied for API call purposes

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync(); //Read body of request


                //Check parameters are supplied
                if ((string.IsNullOrEmpty(requestBody) && string.IsNullOrEmpty(sToken)))
                {
                    log.LogInformation("ERROR ! Parameters not supplied (Require Token and Locator)");
                    oReturn = new ContentResult { Content = "ERROR ! Please supply Bearer Token and Locator or XML Event", ContentType = "text/html; charset=UTF-8", StatusCode = 500 };
                }
                else
                {
                    if (requestBody.Length > 0)
                    {
                        try
                        {

                            //Really shit way to go get the Element data
                            byte[] byteArray = Encoding.ASCII.GetBytes(requestBody);
                            MemoryStream stream = new MemoryStream(byteArray);


                            var xDoc = XDocument.Load(stream);

                            var xElement = xDoc.Root;
                            foreach (var item in xElement.Nodes())
                            {
                                foreach (var item2 in item.NodesAfterSelf())
                                {
                                    XmlReader oXMLReader = item2.CreateReader();
                                    oXMLReader.ReadStartElement();
                                    oXMLReader.ReadStartElement();
                                    sLocator = oXMLReader.ReadInnerXml();
                                }
                            }

                            log.LogInformation("Locator = " + sLocator);

                        }
                        catch (Exception ex)
                        {
                            string ss = ex.Message;
                            log.LogInformation("ERROR ! Unable to get locator from supplied XML ... " + ex.Message);
                            bErrorEncountered = true;
                            throw;
                        }
                    }

                    //Call SOAP Service
                    var oWebservice_Result = InvokeService(sLocator, sToken);


                    //Validate Response is a PNR
                    {
                        //TODO : Since xml namespaces are used in their documents - you need to cater for 

                        //Check for error nodes in XML
                        if (oWebservice_Result.GetElementsByTagName("stl19:Errors").Count > 0 || oWebservice_Result.GetElementsByTagName("soap-env:Fault").Count > 0) //Issues encountered
                        {
                            log.LogInformation("WARNING ! Server response contains errors. Check Error folder on storage for details ...");
                            //log.LogInformation(ConvertXMLtoString(oWebservice_Result));
                            bErrorEncountered = true;
                            oReturn = new ContentResult { Content = Utilities.ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 500 };
                        }
                        else //valid PNR
                        {
                            oReturn = new ContentResult { Content = Utilities.ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 200 };
                        }
                    }


                    //Add Located to Header (Used by Logic App for identification)
                    req.HttpContext.Response.Headers.Add("Locator", sLocator);

                    //Indicate if any errors were detected (Used by Logic App for easier error detection)
                    req.HttpContext.Response.Headers.Add("Errors", bErrorEncountered.ToString());
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                bErrorEncountered = true;
                oReturn = new ContentResult { Content = ex.Message, ContentType = "application/xml", StatusCode = 500 };
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

                var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));

                //var path = System.IO.Path.Combine(context.FunctionDirectory, "Sabre_GetPNR_Payload.xml");

                //Read XML Payload from File
                string sFilename = rootDirectory + @"\Sabre_GetPNR_Payload.xml";

                //Replace placeholders from payload Template
                string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Locator}}", Locator).Replace("{{Token}}",Token);


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
