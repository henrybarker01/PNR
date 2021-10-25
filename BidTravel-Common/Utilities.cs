using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace BidTravel_Common
{
    public class Utilities
    {
        public static string GenerateSabreTokenCredentials(string Username, string Password, string PCC)
        {
            string sReturn = string.Empty;

            try
            {
                //As per documentation from https://developer.sabre.com/guides/travel-agency/developer-guides/rest-apis-token-credentials

                //Build and Encode User ID
                string sUserID = "V1:" + Username.Trim() + ":" + PCC.ToString() + ":AA";
                //string sUserID_Encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sUserID));
                string sUserID_Encoded = Base64Encode(sUserID);

                //Encode Password
                //string sPassword_Encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Password));
                string sPassword_Encoded = Base64Encode(Password);

                //Concat and Encode
                //sReturn = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sUserID_Encoded + ":" + sPassword_Encoded));
                sReturn = Base64Encode(sUserID_Encoded + ":" + sPassword_Encoded);

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


        public static string Base64Encode(string s)
        {
            string base64 = string.Empty;
            try
            {
                var bits = string.Empty;
                foreach (var character in s)
                {
                    bits += Convert.ToString(character, 2).PadLeft(8, '0');
                }



                const byte threeOctets = 24;
                var octetsTaken = 0;
                while (octetsTaken < bits.Length)
                {
                    var currentOctects = bits.Skip(octetsTaken).Take(threeOctets).ToList();

                    const byte sixBits = 6;
                    int hextetsTaken = 0;
                    while (hextetsTaken < currentOctects.Count())
                    {
                        var chunk = currentOctects.Skip(hextetsTaken).Take(sixBits);
                        hextetsTaken += sixBits;

                        var bitString = chunk.Aggregate(string.Empty, (current, currentBit) => current + currentBit);

                        if (bitString.Length < 6)
                        {
                            bitString = bitString.PadRight(6, '0');
                        }
                        var singleInt = Convert.ToInt32(bitString, 2);

                        base64 += Base64Letters[singleInt];
                    }

                    octetsTaken += threeOctets;
                }

                // Pad with = for however many octects we have left
                for (var i = 0; i < (bits.Length % 3); i++)
                {
                    base64 += "=";
                }
            }
            catch { }
            return base64;
        }

        private static readonly char[] Base64Letters = new[]
                                                {
                                              'A'
                                            , 'B'
                                            , 'C'
                                            , 'D'
                                            , 'E'
                                            , 'F'
                                            , 'G'
                                            , 'H'
                                            , 'I'
                                            , 'J'
                                            , 'K'
                                            , 'L'
                                            , 'M'
                                            , 'N'
                                            , 'O'
                                            , 'P'
                                            , 'Q'
                                            , 'R'
                                            , 'S'
                                            , 'T'
                                            , 'U'
                                            , 'V'
                                            , 'W'
                                            , 'X'
                                            , 'Y'
                                            , 'Z'
                                            , 'a'
                                            , 'b'
                                            , 'c'
                                            , 'd'
                                            , 'e'
                                            , 'f'
                                            , 'g'
                                            , 'h'
                                            , 'i'
                                            , 'j'
                                            , 'k'
                                            , 'l'
                                            , 'm'
                                            , 'n'
                                            , 'o'
                                            , 'p'
                                            , 'q'
                                            , 'r'
                                            , 's'
                                            , 't'
                                            , 'u'
                                            , 'v'
                                            , 'w'
                                            , 'x'
                                            , 'y'
                                            , 'z'
                                            , '0'
                                            , '1'
                                            , '2'
                                            , '3'
                                            , '4'
                                            , '5'
                                            , '6'
                                            , '7'
                                            , '8'
                                            , '9'
                                            , '+'
                                            , '/'
                                        };
    }




}
