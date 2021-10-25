using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;

namespace fn_bidtravel_pnrfinisher_pnrhandler_dev
{
    public class BidTravelFOD
    {
        private static string _sFOD_URL;

        public BidTravelFOD(string FOD_URL)
        {
            _sFOD_URL = FOD_URL;
        }


        public string FODURL
        { 
            get { return _sFOD_URL; } 
        }



        public string GetLocale(string Destination, string Group)
        {
            string sReturn = string.Empty;

            try
            {

                string sPayload = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:bid='http://bidtravel.traveport.services'><soapenv:Header/><soapenv:Body><bid:GetLocale><bid:group>" + Group + "</bid:group><bid:limit>99999</bid:limit><bid:debtorCode></bid:debtorCode><bid:destination>" + Destination + "</bid:destination></bid:GetLocale></soapenv:Body></soapenv:Envelope>";

                XmlDocument oResponse = InvokeService("http://bidtravel.traveport.services/ITravelPortService/GetLocale", sPayload);

                sReturn = Utilities.GetXMLValue(oResponse, "GetLocaleResult");

            }
            catch (Exception)
            {

                throw;
            }


            return sReturn;
        }






        private static HttpWebRequest CreateSOAPWebRequest(string SOAPAction)
        {
            //Making Web Request    
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(_sFOD_URL);


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


        private static XmlDocument InvokeService(string SOAPAction, string Payload)
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
                //string sFilename = rootDirectory + @"\FOD_GetLocale_Payload.xml";

                //Replace placeholders from payload Template
                //string sTemplateFileText = File.ReadAllText(sFilename).Replace("{{Group}}", Group).Replace("{{Destination}}", Destination);


                //SOAPReqBody.Load(sFilename);

                SOAPReqBody.LoadXml(Payload);
                
                //SOAPReqBody.LoadXml(sTemplateFileText);
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
                    //string oExceptionMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                    //oReturn.LoadXml(oExceptionMessage);
                    throw;
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
