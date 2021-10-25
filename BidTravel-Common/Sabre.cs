using System;
using System.Collections.Generic;
using System.Text;

namespace BidTravel_Common
{
   static public class Sabre
    {

        public static string GetSabreToken(string SabreUserID, string SabreTokenURL)
        {
            var oReturn = string.Empty;

            try
            {
                ServiceRequest oSoapCall = new ServiceRequest();
                //Add Header Items
                Dictionary<string, string> oHeader = new Dictionary<string, string>();
                oHeader.Add("Authorization", "Basic " + SabreUserID);
                oHeader.Add("Content-Type", "application/x-www-form-urlencoded");
                oHeader.Add("grant_type", "client_credentials");

                //oSoapCall.CreateSOAPWebRequest(oHeader, SOAP.MethodType.POST);

                oReturn = oSoapCall.InvokeRESTService(SabreTokenURL, oHeader, ServiceRequest.MethodType.POST);

            }
            catch (Exception ex)
            {
                throw;
            }

            return oReturn;
        }


        //public static string GetSaberUserID()
        //{


        //}

    }
}
