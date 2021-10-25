using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace fn_bidtravel_pnrfinisher_pnrhandler_dev
{
    static class Utilities
    {
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


        public static string GetRemark(XmlDocument XMLDocument,string StringPrefix, bool ExcludePrefix)
        {
            string sReturn = string.Empty;

            try
            {
                foreach (XmlNode xmlNode in XMLDocument.GetElementsByTagName("stl19:Remark"))
                {
                    if (xmlNode != null)
                    {
                        if (xmlNode.InnerText.ToLower().StartsWith(StringPrefix.ToLower()))
                        {
                            sReturn = xmlNode.InnerText;
                            if (ExcludePrefix == true)
                                sReturn = sReturn.Substring(StringPrefix.Length);
                            break;
                        }
                        //string ss = xmlNode.InnerText;
                    }
                }

                //XmlNode oSearchNode = XMLDocument.GetElementsByTagName("stl19:Remark").Item(0);
                //if (oSearchNode != null)
                //{
                //    sReturn = oSearchNode.InnerText.Trim();
                //}
            }
            catch (Exception)
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
