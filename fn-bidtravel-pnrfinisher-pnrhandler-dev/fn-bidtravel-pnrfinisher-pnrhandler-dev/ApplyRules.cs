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
using System.Collections.Generic;
using System.Text;
using fn_bidtravel_pnrfinisher_pnrhandler_dev;
using System.Xml.Linq;
using System.Data;

namespace fn_bidtravel_pnrfinisher_pnrhandler
{
    public static class ApplyRules
    {
        public enum RemarkAction { Add, Remove, Change }
        public enum RemarkStatus { Added=0, CriteriaNotFound=1, Error=2, AlreadyApplied=3 }

        public struct Remark {

            public RemarkAction Action { get; set; }
            public string Prefix { get; set; }
            public string RemarkText { get; set; }
            public string RemarkID { get; set; }
            public string Status { get; set; }
            public string Notes { get; set; }

        }



        [FunctionName("ApplyRules")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            ContentResult oReturn = new ContentResult();

            //bool bErrorEncountered = false; //Error Flag

            string sToken = req.Headers["Token"]; //Read Token from header (easier as the url encoding is taken care of)
            string sLocator = req.Query["Locator"]; //Optionally supplied for API call purposes
            string sFODURL = req.Headers["FODURL"];

            string sRule = string.Empty;
            if(req.Headers["Rule"].ToString() != "")
                sRule = req.Headers["Rule"]; //OPTIONAL

            if (req.Query["Rule"].ToString() != "")
                sRule = req.Query["Rule"]; //OPTIONAL


            string sRemarkTemplate = string.Empty;
            string sOutputRemark = string.Empty;

            if (String.IsNullOrEmpty(sLocator)) sLocator = req.Headers["Locator"]; //Check if locator was passed via Header?


            sLocator = sLocator.Replace(".xml", string.Empty);
            log.LogInformation("LocatorID = " + sLocator);


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync(); //Read body of request


            //Check parameters are supplied
            if ((string.IsNullOrEmpty(requestBody) && string.IsNullOrEmpty(sToken) && string.IsNullOrEmpty(sFODURL)))
            {
                log.LogInformation("ERROR ! Parameters not supplied (Require Token and Locator and FODURL)");
                oReturn = new ContentResult { Content = "ERROR ! Please supply Bearer Token and Locator and FOD URL", ContentType = "text/html; charset=UTF-8", StatusCode = 500 };
            }
            else
            {
                log.LogInformation("Starting ApplyRules() ...");
                log.LogInformation("Rule to apply : " + sRule);



                //BidTravelFOD oFOD = new BidTravelFOD("http://10.10.40.42/TravelPortService2/TravelPortService.svc");
                //BidTravelFOD oFOD = new BidTravelFOD("https://fodext.bidtravel.co.za/TravelPortService2/TravelPortService.svc");

                BidTravelFOD oFOD = new BidTravelFOD(sFODURL);


                XmlDocument oDoc = new XmlDocument();
                oDoc.LoadXml(requestBody);


                //Get Update Token (Needed for updatePNR Sabre API Call)
                string sUpdateToken = Utilities.GetXMLValue(oDoc, "stl19:UpdateToken");

                List<Remark> oRemarks = new List<Remark>();


                if (!String.IsNullOrEmpty(sUpdateToken))
                {
                    #region Rules


                    #region AGMReference

                    if (sRule.ToLower().Equals("agmreferences") || sRule == string.Empty)
                    {
                        for (int i = 1; i < 11; i++) //There are potentially 10 ACECRM-REFx occurances
                        {
                            Remark oRemark = new Remark();
                            oRemark.RemarkText = string.Empty;
                            oRemark.Notes = string.Empty;
                            oRemark.RemarkID = string.Empty;
                            oRemark.Action = RemarkAction.Add;

                            oRemark.Prefix = "ACECRM-REF" + i + "-";

                            sRemarkTemplate = "{EXISTINGREMARK}*S{SEGMENT}";

                            string sExistingRemarkText = string.Empty;

                            log.LogInformation("Rule : " + oRemark.Prefix);


                            //Search all existing Remarks
                            foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                            {
                                if (nodeItem.InnerText.ToUpper().StartsWith("ACECRM-REF" + i + "-"))
                                {
                                    sExistingRemarkText = nodeItem.InnerText;

                                    break;
                                }
                            }

                            if (sExistingRemarkText.Length > 0) //Only if found
                            {
                                #region Deturmine Segment

                                string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                                #endregion


                                sOutputRemark = sRemarkTemplate.Replace("{EXISTINGREMARK}", sExistingRemarkText).Replace("{SEGMENT}", sSequence);

                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();
                                    //oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }
                                else
                                {
                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();
                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }
                            }
                            else
                            {
                                oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                                oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                                log.LogInformation("Rule Criteria not found...");
                            }



                            //Remark newRemark = new Remark();
                            oRemark.Action = RemarkAction.Change;


                            oRemarks.Add(oRemark);

                        }


                    }

                    #endregion

                    #region Hotel Remarks (ACEHot)

                    if (sRule.ToLower().Equals("acehot") || sRule == string.Empty)
                    {
                        bool bErrorEncountered = false; //Error Flag

                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        sRemarkTemplate = "ACEHOT-ST-{SALESTYPE}-PR-{PRODUCTTYPE}/S{SEGMENT}";
                        oRemark.Prefix = "ACEHOT-";

                        switch (Utilities.GetXMLValue(oDoc, "or114:LineType").ToUpper())
                        {
                            case "HHL": //GDS Hotel (ACEHOT RULE)

                               // log.LogInformation("Condition true to apply ACEHOT Remark Rule ...");


                                #region Deturmine Locale

                                string sLocale = string.Empty;

                                string sCityCode = Utilities.GetXMLValue(oDoc, "or114:HotelCityCode").ToUpper();
                                string sGroupOrBrand = Utilities.GetRemark(oDoc, "GROUP-", true);

                                //Call FOD getLocale()
                                try
                                {
                                    sLocale = oFOD.GetLocale(sCityCode, sGroupOrBrand);
                                }
                                catch (Exception ex)
                                {
                                    log.LogInformation("FOD GetLocale() Failed! (" + oFOD.FODURL + ") - " + ex.Message);
                                    oRemark.Status = RemarkStatus.Error.ToString();
                                    oRemark.Notes = "FOD GetLocale() Failed - " + ex.Message;

                                }

                                #endregion

                                #region Deturmine Product Code

                                string sProductCode = "HO"; //This applies to all hotels

                                #endregion

                                #region Deturmine Sales Type

                                string sSalesType = "___";
                                //Remark does not exist?
                                sSalesType = Utilities.GetRemark(oDoc, "STFIN-ACCDOM-", true);
                                if (sSalesType == string.Empty)
                                {
                                    oRemark.Notes = "STFIN-ACCDOM- missing in PNR";
                                    oRemark.Status = RemarkStatus.Error.ToString();
                                }

                                #endregion

                                #region Deturmine Segment

                                string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");
                                if (sSequence == string.Empty)
                                {
                                    oRemark.Status = RemarkStatus.Error.ToString();
                                    oRemark.Notes = "Unable to deturmine Sequence?";
                                }

                                #endregion

                                if (oRemark.Status != RemarkStatus.Error.ToString() && oRemark.Status != RemarkStatus.CriteriaNotFound.ToString())
                                {

                                    sOutputRemark = sRemarkTemplate.Replace("{SALESTYPE}", sSalesType).Replace("{PRODUCTTYPE}", sProductCode).Replace("{SEGMENT}", sSequence);

                                    //Check if not already applied
                                    if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                    {
                                        log.LogInformation("Remark already exists : " + sOutputRemark);
                                        oRemark.Action = RemarkAction.Change;
                                        oRemark.RemarkText = sOutputRemark;
                                        oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                    }
                                    else
                                    {
                                        log.LogInformation("Remark to add : " + sOutputRemark);
                                        oRemark.Action = RemarkAction.Change;
                                        oRemark.RemarkText = sOutputRemark;
                                        oRemark.Status = RemarkStatus.Added.ToString();
                                        oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                    }
                                }

                                oRemarks.Add(oRemark);


                                break;


                            default: //Passive
                                break;
                        }

                    }


                    #endregion

                    #region HotelCancellation


                    if (sRule.ToLower().Equals("hotelcancellation") || sRule == string.Empty)
                    {
                        Remark oRemark = new Remark(); 
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;
                        sRemarkTemplate = "ACERMK-VOUCH1H-{POLICYTEXT}.*S{SEGMENT}";
                        oRemark.Prefix = "ACERMK-VOUCH1H-";

                        //Get Cancellation Policy Text
                        string sPolicyText = Utilities.GetXMLValue(oDoc, "or114:CancellationPolicy").ToUpper();

                        if (sPolicyText.Length > 0) //Found
                        {

                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }
                            #endregion

                            if (oRemark.Status != RemarkStatus.Error.ToString())
                            {
                                sOutputRemark = sRemarkTemplate.Replace("{POLICYTEXT}", sPolicyText).Replace("{SEGMENT}", sSequence);


                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                }
                                else
                                {
                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();
                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }

                }
                #endregion

                    #region Hotel Saving Remarks

                if (sRule.ToLower().Equals("hotelsavings") || sRule == string.Empty)
                {

                    for (int i = 1; i < 3; i++) //There can be up to 2 Hotel Saving Remarks
                    {
                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        string sExistingRemarkText = string.Empty;

                        sRemarkTemplate = "ACEHV" + i + "-{AMOUNT}-{REASONCODE}-{CURRENCY}*S{SEGMENT}";
                        oRemark.Prefix = "ACEHV" + i + "-";


                        //Search all existing Remarks
                        foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                        {
                            if (nodeItem.InnerText.ToUpper().StartsWith("ACEHV" + i + "-"))
                            {
                                sExistingRemarkText = nodeItem.InnerText;
                                break;
                            }
                        }

                        if (sExistingRemarkText.Length > 0) //Only if found
                        {
                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }
                            #endregion


                            //Split Values
                            string[] sValuesSplit = sExistingRemarkText.Split('-');


                            if (sValuesSplit.Length == 5) //Check correct number of values
                            {
                                string sAmount = sValuesSplit[1];
                                string sReasonCode = sValuesSplit[2] + '-' + sValuesSplit[3];
                                string sCurrency = sValuesSplit[4];


                                sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{CURRENCY}", sCurrency).Replace("{SEGMENT}", sSequence);

                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                }
                                else
                                {

                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();

                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }

                            }
                            else
                            {
                                oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                                oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                                log.LogInformation("Rule Criteria not found...");
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }
                }


                #endregion

                    #region Hotel Vouchers

                if (sRule.ToLower().Equals("hotelvouchers") || sRule == string.Empty)
                {
                    sRemarkTemplate = "{POLICYTEXT}.*S{SEGMENT}";

                    for (int i = 2; i < 5; i++) //There can be up to 3 Hotel Voucher remarks
                    {
                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        oRemark.Prefix = "ACERMK-VOUCH" + i + "H-";

                        string sExistingRemarkText = string.Empty;

                        //Search all existing Remarks
                        foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                        {
                            if (nodeItem.InnerText.ToUpper().StartsWith("ACERMK-VOUCH" + i + "H"))
                            {
                                sExistingRemarkText = nodeItem.InnerText;
                                break;
                            }
                        }

                        if (sExistingRemarkText.Length > 0) //Only if found
                        {
                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }

                            #endregion

                            sOutputRemark = sRemarkTemplate.Replace("{POLICYTEXT}", sExistingRemarkText).Replace("{SEGMENT}", sSequence);


                            //Check if not already applied
                            if (oDoc.InnerText.Contains(sOutputRemark) == true)
                            {
                                log.LogInformation("Remark already exists : " + sOutputRemark);
                                oRemark.Action = RemarkAction.Change;
                                oRemark.RemarkText = sOutputRemark;
                                oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                            }
                            else
                            {
                                log.LogInformation("Remark to add : " + sOutputRemark);
                                oRemark.Action = RemarkAction.Change;
                                oRemark.RemarkText = sOutputRemark;
                                oRemark.Status = RemarkStatus.Added.ToString();

                                oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }

                }

                #endregion

                    #region Air Savings

                if (sRule.ToLower().Equals("airsavings") || sRule == string.Empty)
                {

                    for (int i = 0; i < 2; i++) //There can be up to 2 Air Saving Remarks
                    {
                        string sExistingRemarkText = string.Empty;
                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        if (i == 0)
                            oRemark.Prefix = "ACESV1-";
                        else
                            oRemark.Prefix = "ACESV1A-";

                        sRemarkTemplate = oRemark.Prefix + "{AMOUNT}-{REASONCODE}*S{SEGMENT}";

                        sExistingRemarkText = string.Empty;

                        //Search all existing Remarks
                        foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                        {
                            if (nodeItem.InnerText.ToUpper().StartsWith(oRemark.Prefix))
                            {
                                sExistingRemarkText = nodeItem.InnerText;
                                break;
                            }
                        }

                        if (sExistingRemarkText.Length > 0) //Only if found
                        {
                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Air", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }
                            #endregion

                            //Split Values

                            string[] sValuesSplit = sExistingRemarkText.Split('-');


                            if (sValuesSplit.Length == 3) //Check correct number of values
                            {
                                string sAmount = sValuesSplit[1];
                                string sReasonCode = sValuesSplit[2];

                                sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{SEGMENT}", sSequence);


                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                }
                                else
                                {
                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();
                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }
                            }
                            else
                            {
                                oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                                oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                                log.LogInformation("Rule Criteria not found...");
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }

                }
                #endregion

                #region Car Remarks

                if (sRule.ToLower().Equals("carremarks") || sRule == string.Empty)
                {
                    bool bErrorEncountered = false; //Error Flag

                    Remark oRemark = new Remark();
                    oRemark.RemarkText = string.Empty;
                    oRemark.Notes = string.Empty;
                    oRemark.RemarkID = string.Empty;
                    oRemark.Action = RemarkAction.Add;

                    sRemarkTemplate = "ACECAR-ST-{SALESTYPE}-PR-{PRODUCTTYPE}/S{SEGMENT}";
                    oRemark.Prefix = "ACECAR-";

                    string sSearchCriteria = Utilities.GetXMLAttributeValue(oDoc, "stl19:Vehicle", "DayOfWeekInd");

                    if (sSearchCriteria.Length > 0)
                    {

                        #region Deturmine Locale

                        string sLocale = string.Empty;

                        string sCityCode = Utilities.GetXMLValue(oDoc, "stl19:LocationCode").ToUpper();
                        string sGroupOrBrand = Utilities.GetRemark(oDoc, "GROUP-", true);

                        //Call FOD getLocale()
                        try
                        {
                            sLocale = oFOD.GetLocale(sCityCode, sGroupOrBrand);
                        }
                        catch (Exception ex)
                        {
                            log.LogInformation("FOD GetLocale() Failed! (" + oFOD.FODURL + ") - " + ex.Message);
                            oRemark.Status = RemarkStatus.Error.ToString();
                            oRemark.Notes = "FOD GetLocale() Failed - " + ex.Message;

                        }

                        #endregion

                        #region Deturmine Product Code

                        string sProductCode = "CA"; //This applies to all cars

                        #endregion

                        #region Deturmine Sales Type

                        string sSalesType = "___";
                        //Remark does not exist?
                        sSalesType = Utilities.GetRemark(oDoc, "STFIN-CARDOM-", true);
                        if (sSalesType == string.Empty)
                        {
                            oRemark.Notes = "STFIN-CARDOM- missing in PNR";
                            oRemark.Status = RemarkStatus.Error.ToString();
                        }

                        #endregion

                        #region Deturmine Segment

                        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Vehicle", "sequence");
                        if (sSequence == string.Empty)
                        {
                            oRemark.Status = RemarkStatus.Error.ToString();
                            oRemark.Notes = "Unable to deturmine Sequence?";
                        }

                        #endregion

                        if (oRemark.Status != RemarkStatus.Error.ToString() && oRemark.Status != RemarkStatus.CriteriaNotFound.ToString())
                        {

                            sOutputRemark = sRemarkTemplate.Replace("{SALESTYPE}", sSalesType).Replace("{PRODUCTTYPE}", sProductCode).Replace("{SEGMENT}", sSequence);

                            //Check if not already applied
                            if (oDoc.InnerText.Contains(sOutputRemark) == true)
                            {
                                log.LogInformation("Remark already exists : " + sOutputRemark);
                                oRemark.Action = RemarkAction.Change;
                                oRemark.RemarkText = sOutputRemark;
                                oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                            }
                            else
                            {
                                log.LogInformation("Remark to add : " + sOutputRemark);
                                oRemark.Action = RemarkAction.Change;
                                oRemark.RemarkText = sOutputRemark;
                                oRemark.Status = RemarkStatus.Added.ToString();
                                oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                            }
                        }

                        oRemarks.Add(oRemark);


                    }

                }


                #endregion

                #region Car Savings

                if (sRule.ToLower().Equals("carsavings") || sRule == string.Empty)
                {

                    for (int i = 0; i < 2; i++) //There can be up to 2 Car Saving Remarks
                    {
                        string sExistingRemarkText = string.Empty;
                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        if (i == 0)
                            oRemark.Prefix = "ACECV1-";
                        else
                            oRemark.Prefix = "ACECV2-";

                        sRemarkTemplate = oRemark.Prefix + "{AMOUNT}-M-CU-{CURRENCY}*S{SEGMENT}";


                        sExistingRemarkText = string.Empty;

                        //Search for criteria
                        sExistingRemarkText = Utilities.GetXMLValue(oDoc,"stl19:VehicleChargeAmount");
        

                        if (sExistingRemarkText.Length > 0) //Only if found
                        {
                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Vehicle", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }
                            #endregion

                            //Split Values

                            string[] sValuesSplit = sExistingRemarkText.Split(' ');


                            if (sValuesSplit.Length > 3) //Check correct number of values
                            {
                                string sAmount = sValuesSplit[0];
                                string sCurrency = "ZAR"; // sValuesSplit[2]; FIX ... get value from string

                                sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{CURRENCY}", sCurrency).Replace("{SEGMENT}", sSequence);


                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                }
                                else
                                {
                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();
                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }
                            }
                            else
                            {
                                oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                                oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                                log.LogInformation("Rule Criteria not found...");
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }

                }



                #endregion


                #region Air Saving Remarks

                if (sRule.ToLower().Equals("airsavingsremarks") || sRule == string.Empty)
                {

                    for (int i = 3; i < 5; i++) //There can be up to 2 Air Saving Remarks
                    {
                        Remark oRemark = new Remark();
                        oRemark.RemarkText = string.Empty;
                        oRemark.Notes = string.Empty;
                        oRemark.RemarkID = string.Empty;
                        oRemark.Action = RemarkAction.Add;

                        string sExistingRemarkText = string.Empty;

                        sRemarkTemplate = "ACESV" + i + "A-{AMOUNT}-{REASONCODE}-*S{SEGMENT}";
                        oRemark.Prefix = "ACEHV" + i + "-";


                        //Search all existing Remarks
                        foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                        {
                            if (nodeItem.InnerText.ToUpper().StartsWith("ACESV" + i + "A-"))
                            {
                                sExistingRemarkText = nodeItem.InnerText;
                                break;
                            }
                        }

                        if (sExistingRemarkText.Length > 0) //Only if found
                        {
                            #region Deturmine Segment

                            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Air", "sequence");
                            if (sSequence == string.Empty)
                            {
                                oRemark.Status = RemarkStatus.Error.ToString();
                                oRemark.Notes = "Unable to deturmine Sequence?";
                            }
                            #endregion


                            //Split Values
                            string[] sValuesSplit = sExistingRemarkText.Split('-');


                            if (sValuesSplit.Length == 3) //Check correct number of values
                            {
                                string sAmount = sValuesSplit[1];
                                string sReasonCode = sValuesSplit[2];


                                sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{SEGMENT}", sSequence);

                                //Check if not already applied
                                if (oDoc.InnerText.Contains(sOutputRemark) == true)
                                {
                                    log.LogInformation("Remark already exists : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.AlreadyApplied.ToString();

                                }
                                else
                                {

                                    log.LogInformation("Remark to add : " + sOutputRemark);
                                    oRemark.Action = RemarkAction.Change;
                                    oRemark.RemarkText = sOutputRemark;
                                    oRemark.Status = RemarkStatus.Added.ToString();

                                    oRemark.RemarkID = GetLastRemarkID(oDoc, oRemark.Prefix);
                                }

                            }
                            else
                            {
                                oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                                oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                                log.LogInformation("Rule Criteria not found...");
                            }
                        }
                        else
                        {
                            oRemark.Status = RemarkStatus.CriteriaNotFound.ToString();
                            oRemark.Notes = "Prefix : " + oRemark.Prefix + " not found";
                            log.LogInformation("Rule Criteria not found...");
                        }

                        oRemarks.Add(oRemark);
                    }
                }




                #endregion

                #endregion







                //     switch (sRule.ToLower())
                //         {
                //case "agmreferences4":

                //    sRemarkTemplate = "{EXISTINGREMARK}*S{SEGMENT}";
                //    oRemark.Prefix = "ACECRM-REF4-";

                //    string sExistingRemarkText = string.Empty;

                //    //Search all existing Remarks
                //    foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //    {
                //        if (nodeItem.InnerText.ToUpper().StartsWith("ACECRM-REF4-"))
                //        {
                //            sExistingRemarkText = nodeItem.InnerText;

                //            break;
                //        }
                //    }

                //    if (sExistingRemarkText.Length > 0) //Only if found
                //    {
                //        #region Deturmine Segment

                //        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //        #endregion


                //        sOutputRemark = sRemarkTemplate.Replace("{EXISTINGREMARK}", sExistingRemarkText).Replace("{SEGMENT}", sSequence);
                //    }
                //    else
                //    {

                //    }


                //    break;




                //case "acehot":

                //    sRemarkTemplate = "ACEHOT-ST-{SALESTYPE}-PR-{PRODUCTTYPE}/S{SEGMENT}";
                //    oRemark.Prefix = "ACEHOT-";

                //    switch (Utilities.GetXMLValue(oDoc, "or114:LineType").ToUpper())
                //    {
                //        case "HHL": //GDS Hotel (ACEHOT RULE)

                //            log.LogInformation("Condition true to apply ACEHOT Remark Rule ...");


                //            #region Deturmine Locale

                //            string sLocale = string.Empty;

                //            string sCityCode = Utilities.GetXMLValue(oDoc, "or114:HotelCityCode").ToUpper();
                //            string sGroupOrBrand = Utilities.GetRemark(oDoc, "GROUP-", true);

                //            //Call FOD getLocale()
                //            try
                //            {
                //                sLocale = oFOD.GetLocale(sCityCode, sGroupOrBrand);
                //            }
                //            catch (Exception ex)
                //            {
                //                log.LogInformation("FOD GetLocale() Failed! (" + oFOD.FODURL + ") - " + ex.Message);
                //                bErrorEncountered = true;
                //            }

                //            #endregion

                //            #region Deturmine Product Code

                //            string sProductCode = "HO"; //This applies to all hotels

                //            #endregion

                //            #region Deturmine Sales Type

                //            string sSalesType = "___";
                //            //Remark does not exist?
                //            sSalesType = Utilities.GetRemark(oDoc, "STFIN-ACCDOM-", true);

                //            #endregion

                //            #region Deturmine Segment

                //            string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //            #endregion


                //            sOutputRemark = sRemarkTemplate.Replace("{SALESTYPE}", sSalesType).Replace("{PRODUCTTYPE}", sProductCode).Replace("{SEGMENT}", sSequence);

                //            break;


                //        default: //Passive
                //            break;
                //    }

                //    break;

                //case "hotelcancellation":

                //    sRemarkTemplate = "ACERMK-VOUCH1H-{POLICYTEXT}.*S{SEGMENT}";
                //    oRemark.Prefix = "ACERMK-VOUCH1H-";

                //    //Get Cancellation Policy Text
                //    string sPolicyText = Utilities.GetXMLValue(oDoc, "or114:CancellationPolicy").ToUpper();

                //    if(sPolicyText.Length > 0) //Found
                //    {

                //        #region Deturmine Segment

                //        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //        #endregion


                //        sOutputRemark = sRemarkTemplate.Replace("{POLICYTEXT}", sPolicyText).Replace("{SEGMENT}", sSequence);

                //    }

                //    break;

                //case "hotelvoucher2":

                //    sRemarkTemplate = "{POLICYTEXT}.*S{SEGMENT}";
                //    oRemark.Prefix = "ACERMK-VOUCH2H-";

                //    sExistingRemarkText = string.Empty;

                //    //Search all existing Remarks
                //    foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //    {
                //        if(nodeItem.InnerText.ToUpper().StartsWith("ACERMK-VOUCH2H"))
                //        {
                //            sExistingRemarkText = nodeItem.InnerText;
                //            break;
                //        }
                //    } 

                //    if(sExistingRemarkText.Length > 0) //Only if found
                //    {
                //        #region Deturmine Segment

                //        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //        #endregion

                //        sOutputRemark = sRemarkTemplate.Replace("{POLICYTEXT}", sExistingRemarkText).Replace("{SEGMENT}", sSequence);
                //    }


                //    break;


                //     case "hotelsavings1":

                //sRemarkTemplate = "ACEHV1-{AMOUNT}-{REASONCODE}-{CURRENCY}*S{SEGMENT}";
                //oRemark.Prefix = "ACEHV1-";

                //sExistingRemarkText = string.Empty;

                ////Search all existing Remarks
                //foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //{
                //    if (nodeItem.InnerText.ToUpper().StartsWith("ACEHV1-"))
                //    {
                //        sExistingRemarkText = nodeItem.InnerText;

                //        break;
                //    }
                //}

                //if (sExistingRemarkText.Length > 0) //Only if found
                //{
                //    #region Deturmine Segment

                //    string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //    #endregion

                //    //Split Values

                //    string[] sValuesSplit = sExistingRemarkText.Split('-');


                //    if (sValuesSplit.Length == 5) //Check correct number of values
                //    {
                //        string sAmount = sValuesSplit[1];
                //        string sReasonCode = sValuesSplit[2] + '-' + sValuesSplit[3];
                //        string sCurrency = sValuesSplit[4];

                //        sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}",sReasonCode).Replace("{CURRENCY}",sCurrency).Replace("{SEGMENT}", sSequence);
                //    }
                //}


                //   break;

                //case "hotelsavings2":

                //    sRemarkTemplate = "ACEHV2-{AMOUNT}-{REASONCODE}-{CURRENCY}*S{SEGMENT}";
                //    oRemark.Prefix = "ACEHV2-";

                //    sExistingRemarkText = string.Empty;

                //    //Search all existing Remarks
                //    foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //    {
                //        if (nodeItem.InnerText.ToUpper().StartsWith("ACEHV2-"))
                //        {
                //            sExistingRemarkText = nodeItem.InnerText;

                //            break;
                //        }
                //    }

                //    if (sExistingRemarkText.Length > 0) //Only if found
                //    {
                //        #region Deturmine Segment

                //        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Hotel", "sequence");

                //        #endregion

                //        //Split Values

                //        string[] sValuesSplit = sExistingRemarkText.Split('-');


                //        if (sValuesSplit.Length == 5) //Check correct number of values
                //        {
                //            string sAmount = sValuesSplit[1];
                //            string sReasonCode = sValuesSplit[2] + '-' + sValuesSplit[3];
                //            string sCurrency = sValuesSplit[4];

                //            sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{CURRENCY}", sCurrency).Replace("{SEGMENT}", sSequence);
                //        }
                //    }


                //    break;



                //case "airsavings1":

                //    sRemarkTemplate = "ACESV1-{AMOUNT}-{REASONCODE}*S{SEGMENT}";
                //    oRemark.Prefix = "ACESV1-";

                //    sExistingRemarkText = string.Empty;

                //    //Search all existing Remarks
                //    foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //    {
                //        if (nodeItem.InnerText.ToUpper().StartsWith("ACESV1-"))
                //        {
                //            sExistingRemarkText = nodeItem.InnerText;

                //            break;
                //        }
                //    }

                //    if (sExistingRemarkText.Length > 0) //Only if found
                //    {
                //        #region Deturmine Segment

                //        string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Air", "sequence");

                //        #endregion

                //        //Split Values

                //        string[] sValuesSplit = sExistingRemarkText.Split('-');


                //        if (sValuesSplit.Length == 3) //Check correct number of values
                //        {
                //            string sAmount = sValuesSplit[1];
                //            string sReasonCode = sValuesSplit[2] ;
                //            //string sCurrency = sValuesSplit[4];

                //            sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{SEGMENT}", sSequence);
                //        }
                //    }


                //    break;



                // case "carsavings1":

                //sRemarkTemplate = "ACESV1-{AMOUNT}-{REASONCODE}*S{SEGMENT}";
                //oRemark.Prefix = "ACESV1-";

                //sExistingRemarkText = string.Empty;

                ////Search all existing Remarks
                //foreach (XmlNode nodeItem in oDoc.GetElementsByTagName("stl19:Text"))
                //{
                //    if (nodeItem.InnerText.ToUpper().StartsWith("ACESV1-"))
                //    {
                //        sExistingRemarkText = nodeItem.InnerText;

                //        break;
                //    }
                //}

                //if (sExistingRemarkText.Length > 0) //Only if found
                //{
                //    #region Deturmine Segment

                //    string sSequence = Utilities.GetXMLAttributeValue(oDoc, "stl19:Air", "sequence");

                //    #endregion

                //    //Split Values

                //    string[] sValuesSplit = sExistingRemarkText.Split('-');


                //    if (sValuesSplit.Length == 3) //Check correct number of values
                //    {
                //        string sAmount = sValuesSplit[1];
                //        string sReasonCode = sValuesSplit[2];
                //        //string sCurrency = sValuesSplit[4];

                //        sOutputRemark = sRemarkTemplate.Replace("{AMOUNT}", sAmount).Replace("{REASONCODE}", sReasonCode).Replace("{SEGMENT}", sSequence);
                //    }
                //}


                //     break;






                //     }






                //Only Add if no errors encountered
                //  if (bErrorEncountered == false)
                //  {
                //      log.LogInformation("Remark to add : " + sOutputRemark);

                //Remark newRemark = new Remark();
                //      oRemark.Action = RemarkAction.Change;

                //      oRemark.RemarkText = sOutputRemark;




                ////Get Remark ID to update in PNR (Not an elegant way of doing this!)
                //foreach (XmlNode parentNode in oDoc.GetElementsByTagName("stl19:Remark"))
                //{
                //    if (oRemark.RemarkID != null)
                //        break;

                //    foreach (XmlNode childNode in parentNode.ChildNodes)
                //    {
                //        foreach (XmlNode childChildNode in parentNode.ChildNodes)
                //        {
                //            if (childChildNode.InnerText.ToUpper().Contains(oRemark.Prefix.ToUpper()))
                //            {
                //                oRemark.RemarkID = parentNode.Attributes["id"].Value;
                //                break;
                //            }
                //        }
                //    }
                //}
                //}
                //else
                //{
                //    log.LogInformation("ERROR! No remark added due to errors!");
                //}






                //Add Located to Header (Used by Logic App for identification)
                req.HttpContext.Response.Headers.Add("Locator", sLocator);

                //Indicate if any errors were detected (Used by Logic App for easier error detection)

                
               //req.HttpContext.Response.Headers.Add("Errors", bErrorEncountered.ToString());


                //Convert Payload to Json
                oReturn.Content = JsonConvert.SerializeObject(oRemarks);

            }

            return oReturn;
        }





        public static string GetLastRemarkID(XmlDocument xmlDocument, string ElementTagName)
        {
            string sReturn = string.Empty;

            try
            {
                //Get Remark ID to update in PNR (Not an elegant way of doing this!)
                foreach (XmlNode parentNode in xmlDocument.GetElementsByTagName("stl19:Remark"))
                {
                    //if (oRemark.RemarkID != null)
                    //    break;

                    foreach (XmlNode childNode in parentNode.ChildNodes)
                    {
                        foreach (XmlNode childChildNode in parentNode.ChildNodes)
                        {
                            //if (childChildNode.InnerText.ToUpper().Contains(oRemark.Prefix.ToUpper()))
                            if (childChildNode.InnerText.ToUpper().Contains(ElementTagName.ToUpper()))
                            {
                                sReturn = parentNode.Attributes["id"].Value;
                                break;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw;
            }


            return sReturn;
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
