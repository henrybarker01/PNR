using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;


namespace BidTravel_Common
{
    public  class ServiceRequest
    {
        public enum MethodType { POST, GET }

        private  HttpWebRequest CreateSOAPWebRequest(string URL, Dictionary<string,string> HeaderItems, MethodType Method, string SOAPAction="")
        {
            //Making Web Request    
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(URL);

            try
            {
                //Add Headers
                foreach (var item in HeaderItems)
                {
                    oWebRequest.Headers.Add(item.Key, item.Value);
                }
                //oWebRequest.ContentType = "application/x-www-form-urlencoded";

                if (SOAPAction.Length > 0) oWebRequest.Headers.Add(@"SOAPAction:" + SOAPAction);
                //SOAPAction we want to call    
                //oWebRequest.Headers.Add(@"SOAPAction:getReservationRQ");

                //Content_type    
                //oWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
                //oWebRequest.Accept = "text/xml";

                //HTTP method    
                oWebRequest.Method = Method.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return oWebRequest;
        }

        public string InvokeRESTService(string URL, Dictionary<string, string> HeaderItems, MethodType Method)
        {
            string sReturn = string.Empty;


            try
            {
                WebResponse Service = null;

                HttpWebRequest request = CreateSOAPWebRequest(URL, HeaderItems, Method);

                //using (Stream stream = request.GetRequestStream())
                //{
                //    Body.Save(stream);
                //}
                try
                {
                    using (Service = request.GetResponse())
                    {
                        using (StreamReader rd = new StreamReader(Service.GetResponseStream()))
                        {
                            //reading stream    
                            sReturn = rd.ReadToEnd();
                            //writting stream result on console    

                             //ServiceResult);

                            //Console.WriteLine(ServiceResult);
                            //Console.ReadLine();
                        }
                    }
                }
                catch (WebException ex)
                {
                    string oExceptionMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                    sReturn = oExceptionMessage;
                    //oReturn.LoadXml(oExceptionMessage);
                    //log.LogInformation("ERROR ! - unable to call sabre web service");
                }
            }
            catch (Exception)
            {

                throw;
            }


            return sReturn;
        }

        public  XmlDocument InvokeSOAPService(string URL, Dictionary<string, string> HeaderItems, MethodType Method, XmlDocument Body, string SOAPAction="")
        {
            XmlDocument oReturn = new XmlDocument();

            WebResponse Serviceres = null;
            try
            {
                //Call CreateSOAPWebRequest method    
                HttpWebRequest request = CreateSOAPWebRequest(URL,HeaderItems,Method,SOAPAction);

                //XmlDocument SOAPReqBody = new XmlDocument();

                var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var rootDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));


                //Read XML Payload from File
                //string sFilename = rootDirectory + @"\Sabre_GetPNR_Payload.xml";

                //Replace placeholders from payload Template
                //string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Locator}}", Locator).Replace("{{Token}}", Token);


                //SOAPReqBody.Load(sFilename);
                //SOAPReqBody.LoadXml(sTemplateFileText);
                using (Stream stream = request.GetRequestStream())
                {
                   Body.Save(stream);
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
        public XmlDocument InvokeSOAPService(string URL, Dictionary<string, string> HeaderItems, MethodType Method)
        {
            return InvokeSOAPService(URL,HeaderItems,Method, new XmlDocument(), "");
        }

    }
}
