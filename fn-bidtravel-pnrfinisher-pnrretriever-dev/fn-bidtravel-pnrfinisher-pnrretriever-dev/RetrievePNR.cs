using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceReference_Sabre_GetReservation;
using System.Net;
using System.Xml;
using System.Reflection;
using System.Xml.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage;

namespace fn_bidtravel_pnrfinisher_pnrretriever_dev
{
    public static class RetrievePNR
    {
        [FunctionName("RetrieveItinerary")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {

            ContentResult oReturn = new ContentResult();

            try
            {
                //Get Parameters 
                string sToken = req.Query["Token"];
                string sLocator = req.Query["Locator"];
                string sEventXML = string.Empty;


                StreamReader oBodyStream = new StreamReader(req.Body);
                //oBodyStream.ReadToEnd();
                
                sEventXML = oBodyStream.ReadToEnd();



                log.LogInformation("Locator = " + sLocator);

                //Check parameters are supplied
                if ((string.IsNullOrEmpty(sEventXML) && string.IsNullOrEmpty(sToken)) || string.IsNullOrEmpty(sToken))
                {
                    log.LogInformation("ERROR ! Parameters not supplied (Require Token and Locator)");
                    oReturn = new ContentResult { Content = "ERROR ! Please supply Bearer Token and Locator or XML Event", ContentType = "text/html; charset=UTF-8", StatusCode = 500 };
                }
                else
                {
                    if(sEventXML.Length > 0)
                    {
                        try
                        {

                            //Really shit way to go get the Element data
                            byte[] byteArray = Encoding.ASCII.GetBytes(sEventXML);
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
                        }
                        catch (Exception ex)
                        {
                            string ss = ex.Message;
                            log.LogInformation("ERROR ! Unable to get locator from supplied XML ... " + ex.Message);
                            throw;
                        }

                    }



                    //Call SOAP Service
                    var oWebservice_Result = InvokeService(sLocator, sToken);


                    //Validate Response is a PNR
                    {
                        //TODO : Since xml namespaces are used in their documents - you need to cater for 


                        XmlNodeList oElemList = oWebservice_Result.GetElementsByTagName("stl19:Errors");
                        if (oElemList.Count > 0) //Possible invalid PNR
                        {
                            log.LogInformation("WARNING ! Server response contains errors. Check Error folder on storage for details ...");
                            log.LogInformation(ConvertXMLtoString(oWebservice_Result));
                            oReturn = new ContentResult { Content = ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 500 };
                        }
                        else //valid PNR
                        {
                            oReturn = new ContentResult { Content = ConvertXMLtoString(oWebservice_Result), ContentType = "application/xml", StatusCode = 200 };
                        }
                    }


                    //Add Located to Header (Used by Logic App for identification)
                    req.HttpContext.Response.Headers.Add("Locator", sLocator);
                }

            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
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
            catch(Exception ex)
            {
                throw ex;
            }


            return oWebRequest;
        }


        public static XmlDocument InvokeService(string Locator, string Token)
        {
            XmlDocument oReturn = new XmlDocument();

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
                string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Locator}}", Locator);


                //SOAPReqBody.Load(sFilename);
                SOAPReqBody.LoadXml(sTemplateFileText);
                using (Stream stream = request.GetRequestStream())
                {
                    SOAPReqBody.Save(stream);
                }
                //Geting response from request    
                using (WebResponse Serviceres = request.GetResponse())
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
            catch(Exception ex)
            {
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



        public static void WriteToBlob()
        {
            //CloudStorageAccount storageAccount = GetCloudStorageAccount(context);


        }
    }
}
