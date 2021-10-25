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

namespace fn_bidtravel_pnrfinisher_rulehandler
{
    public static class ApplyRules
    {
        [FunctionName("ApplyRules")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            bool bErrorEncountered = false; //Error Flag

            string sToken = req.Headers["Token"]; //Read Token from header (easier as the url encoding is taken care of)
            string sLocator = req.Query["Locator"]; //Optionally supplied for API call purposes
            if (String.IsNullOrEmpty(sLocator)) sLocator = req.Headers["Locator"];

            string sRule = req.Headers["Rule"];



            string requestBody = await new StreamReader(req.Body).ReadToEndAsync(); //Read body of request



            XmlDocument oDoc = new XmlDocument();
            oDoc.LoadXml(requestBody);

            //Get Update Token (Needed for updatePNR API Call)
            string sUpdateToken = GetXMLValue(oDoc, "stl19:UpdateToken");

            string sRemarkTemplate = string.Empty;
            string sOutputRemark = string.Empty;


            if (!String.IsNullOrEmpty(sUpdateToken))
            {

                switch (sRule.ToLower())
                {
                    case "acehot":

                        //Set Remark Template
                        sRemarkTemplate = "ACEHOT-ST-{SALESTYPE}-PR-{PRODUCTTYPE}/S{SEGMENT}";

                        switch (GetXMLValue(oDoc, "or114:LineType").ToUpper())
                        {
                            case "HHL": //GDS Hotel

                                #region Deturmine Locale

                                string sLocale = "DOM";

                                string sCityCode = GetXMLValue(oDoc, "or114:HotelCityCode").ToUpper();
                                string sDeptorAccountNumber = "";
                                string sGroupOrBrand = "";

                                //Call FOD getLocale()

                                #endregion

                                #region Deturmine Product Code

                                string sProductCode = "???";

                                //Call FOD getProductCode()

                                #endregion

                                #region Deturmine Sales Type

                                string sSalesType = "???";
                                //Remark does not exist?

                                #endregion

                                #region Deturmine Segment
                                string sSequence = GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");


                                #endregion


                                sOutputRemark = sRemarkTemplate.Replace("{SALESTYPE}", sSalesType).Replace("{PRODUCTTYPE}", sProductCode).Replace("{SEGMENT}", sSequence);

                                break;


                            default: //Passive
                                break;
                        }

                        


                        break;


                    case "":

                        break;
                }


                

                //Add Located to Header (Used by Logic App for identification)
                req.HttpContext.Response.Headers.Add("Locator", sLocator);

                //Indicate if any errors were detected (Used by Logic App for easier error detection)
                req.HttpContext.Response.Headers.Add("Errors", bErrorEncountered.ToString());

            }

            return oReturn;
        }


        public static string GetXMLValue(XmlDocument XMLDocument, string ElementTagName)
        {
            string sReturn = string.Empty;

            try
            {
                XmlNode oSearchNode = XMLDocument.GetElementsByTagName(ElementTagName).Item(0);
                if (oSearchNode != null)
                {
                    sReturn = oSearchNode.InnerText.Trim();
                }
            }
            catch (Exception ex)
            {

                throw;
            }


            return sReturn;
        }

        //public static string GetNodeByValue(XmlDocument XMLDocument, string Value, bool PartialMatch=true)
        //{
        //    string sReturn = string.Empty;

        //    try
        //    {
        //        //XmlNode oSearchNode = XMLDocument.getGetElementsByTagName(ElementTagName).Item(0);
        //        if (oSearchNode != null)
        //        {
        //            sReturn = oSearchNode.InnerText.Trim();
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }


        //    return sReturn;
        //}


        public static string GetXMLAttributeValue(XmlDocument XMLDocument, string ElementTagName, string AttributeName)
        {
            string sReturn = string.Empty;

            try
            {
                XmlNode oSearchNode = XMLDocument.GetElementsByTagName(ElementTagName).Item(0);
                if (oSearchNode != null)
                {
                    sReturn = oSearchNode.Attributes[AttributeName].Value.Trim();
                }
            }
            catch (Exception)
            {

                throw;
            }


            return sReturn;
        }

    }
}
