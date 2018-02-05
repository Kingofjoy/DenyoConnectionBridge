using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.DataStructures
{
    public static class Converter
    {
        public static string HexaToString(string Hexavalue,string UnitCode)
        {
            string sHexaValue = string.Empty;
            try
            {
                Hexavalue = Hexavalue.Trim();
                //Console.WriteLine("HexaToString: "+Hexavalue + " : " + UnitCode);
                if(UnitCode == "A")
                {
                    sHexaValue = AlarmHexaToString(Hexavalue);
                    Console.WriteLine("HexaToString: " + Hexavalue + " = " + UnitCode + " = " + sHexaValue);
                    return sHexaValue;
                }
                sHexaValue = HexaToString(Hexavalue);
                //Console.WriteLine("HexaToString: " + Hexavalue + " : " + UnitCode + " = " + sHexaValue);

                if (UnitCode=="FREQ" || UnitCode == "COLLANTTEMP" || UnitCode == "BATTERYVOLTAGE" || UnitCode == "OILPRESSURE")
                {

                    sHexaValue = (((Decimal.Parse(sHexaValue) / 10))).ToString();
                    if(sHexaValue.IndexOf(".")>=0)
                    sHexaValue = sHexaValue.Substring(0, sHexaValue.IndexOf(".") + 2);
                    //sHexaValue = (Math.Round( (Decimal.Parse(sHexaValue) / 10) , 1 )).ToString();
                }
                else if(UnitCode == "RUNNINGHR")
                {
                    if (sHexaValue.Length >= 7)
                        sHexaValue = (Decimal.Parse(sHexaValue) * 1000).ToString();
                    else if (sHexaValue.Length >= 3)
                        sHexaValue = (Decimal.Parse(sHexaValue) * 100).ToString();
                    else
                        sHexaValue = (Decimal.Parse(sHexaValue) * 10).ToString();

                    if (sHexaValue != "0")
                        sHexaValue = Math.Round(Double.Parse(sHexaValue), 2).ToString();
                }
                else if(UnitCode == "FUELLEVEL" || UnitCode == "ENGSPEED" || UnitCode == "VOLT1" || UnitCode == "VOLT2" || UnitCode == "VOLT3" || UnitCode == "CURRENT1" || UnitCode == "CURRENT2" || UnitCode == "CURRENT3" || UnitCode == "!!ENGSPEED" || UnitCode == "LOADPOWER1" || UnitCode == "LOADPOWER2" || UnitCode == "LOADPOWER3")
                {
                    sHexaValue = Math.Round(Decimal.Parse(sHexaValue)).ToString();
                }
                else if(UnitCode == "ENGINESTATE" || UnitCode == "MODE")
                {                    
                    sHexaValue = Math.Ceiling(Decimal.Parse(sHexaValue)).ToString();
                }
            }
            catch(Exception conex)
            {
                Console.WriteLine("HexCon Err:"+conex.Message+". Data:"+Hexavalue+ ". UnitCode: "+UnitCode);
            }
            //Console.WriteLine("HexaToString: " + Hexavalue + " = " + UnitCode + " = " + sHexaValue);
            return sHexaValue;
        }

        public static string AlarmHexaToString(string Hexavalue)
        {
            string strValue = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(Hexavalue)) return Hexavalue;

                string[] hexapairs = new string[(Hexavalue.Length / 2) + 1];
                if (Hexavalue.IndexOf(" ") > 0)
                {
                    //Hexavalue = Hexavalue.Substring(6);
                    //Hexavalue = Hexavalue.Substring(0, Hexavalue.Length - 6);
                    hexapairs = Hexavalue.Split(" ".ToCharArray());
                }
                else
                {
                    //Hexavalue = Hexavalue.Substring(4);
                    //Hexavalue = Hexavalue.Substring(0, Hexavalue.Length - 4);

                    for (int i = 0; i <= (Hexavalue.Length / 2); i += 2)
                        hexapairs[i / 2] = Hexavalue.Substring(i, 2);

                    if (Hexavalue.Length % 2 != 0)
                        hexapairs[Hexavalue.Length / 2] = Hexavalue.Substring(Hexavalue.Length - 1);
                }

                string sHexSet;

                sHexSet = string.Join("", hexapairs.Skip(8).Take(14));

                byte[] data = new byte[sHexSet.Length / 2];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Convert.ToByte(sHexSet.Substring(i * 2, 2), 16);
                }
                strValue = Encoding.ASCII.GetString(data).Replace("\0", "");
                    //Console.WriteLine("strValue " + sHexSet1 + "HexNumber: " + strValue);
               
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("HexToString Err:" + ex.Message + ". Data: " + Hexavalue);
                strValue = Hexavalue;

            }
            return strValue;
        }

        public static string HexaToString(string Hexavalue)
        {
            string strValue = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(Hexavalue)) return Hexavalue;

                string[] hexapairs = new string[(Hexavalue.Length / 2)+1];
                if (Hexavalue.IndexOf(" ")>0)
                {
                    //Hexavalue = Hexavalue.Substring(6);
                    //Hexavalue = Hexavalue.Substring(0, Hexavalue.Length - 6);
                    hexapairs = Hexavalue.Split(" ".ToCharArray());
                }
                else
                {
                    //Hexavalue = Hexavalue.Substring(4);
                    //Hexavalue = Hexavalue.Substring(0, Hexavalue.Length - 4);

                    for (int i = 0; i <= (Hexavalue.Length / 2); i += 2)
                        hexapairs[i/2] = Hexavalue.Substring(i, 2);

                    if (Hexavalue.Length % 2 != 0)
                        hexapairs[Hexavalue.Length / 2] = Hexavalue.Substring(Hexavalue.Length - 1);
                }

                string sHexSet1, sHexSet2;

                //Console.WriteLine(Hexavalue + " length " + hexapairs.Length); 
                if (hexapairs.Length==7)
                {
                    sHexSet1 = hexapairs[3] + hexapairs[4];
                    //sHexSet2 = hexapairs[5] + hexapairs[6];
                    strValue = Convert.ToInt64(sHexSet1, 16).ToString();
                    //Console.WriteLine("strValue "+ sHexSet1 + " Int64: "  + strValue);
                    strValue = int.Parse(sHexSet1, System.Globalization.NumberStyles.HexNumber).ToString();
                    //Console.WriteLine("strValue " + sHexSet1 + "HexNumber: " + strValue);
                }
                else 
                {
                    sHexSet1 = hexapairs[3] + hexapairs[4];
                    sHexSet2 = hexapairs[5] + hexapairs[6];
                    strValue = int.Parse(sHexSet1, System.Globalization.NumberStyles.HexNumber).ToString()
                              +"."+ int.Parse(sHexSet2, System.Globalization.NumberStyles.HexNumber).ToString();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("HexToString Err:" + ex.Message+ ". Data: "+Hexavalue);
                strValue = Hexavalue;

            }
            return strValue;
        }
    }
}
