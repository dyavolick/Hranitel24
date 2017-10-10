using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpListener.Mt90
{
    public class Mt90StringHelper
    {
        public string GetChecksums(string s)
        {
            var i = s.Sum(a => (int)a);
            string myHex = i.ToString("X");
            var aOut = myHex.Substring(myHex.Length - 2);
            return aOut;
        }

        public Mt90Message GetMt90Message(string s)
        {
            var message = new Mt90Message();
            //"$$B145,863158020721157,AAA,35,52.691971,25.372790,150204170317,V,0,8,0,0,0.0,0,7067,150931,257|1|00FE|066A,0000,0000|0000|0000|09E9|0000,00000001,*"
            if (!String.IsNullOrEmpty(s) && s.IndexOf(',') > -1)
            {
                var messageArr = s.Split(',');
                if (messageArr.Length > 3 && messageArr[0].StartsWith("$$"))
                {
                    var message0 = messageArr[0].Remove(0, 2);
                    message.DataIdentifier = message0.Remove(1);
                    message.DataLength = Convert.ToInt32(message0.Remove(0, 1));
                    message.Imei = messageArr[1];
                    message.CommandType = messageArr[2];
                 

               
                    if (message.CommandType == "AAA")
                    {
                        message.EventCode = messageArr[3];
                        message.Latitude = messageArr[4];
                        message.Longitude = messageArr[5];
                    
                       
                        string formatString = "yy''MM''dd''HH''mm''ss";
                        string sampleData =messageArr[6];;
                        message.DateAndTime = DateTime.ParseExact(sampleData,formatString,  null);

                        message.PositioningStatus = messageArr[7];
                        message.NumberOfSatellites = Convert.ToInt32(messageArr[8]);
                        message.GsmSignalStrength = Convert.ToInt32(messageArr[9]);
                        message.Speed = messageArr[10];
                        message.Direction = Convert.ToInt32(messageArr[11]);
                        message.HorizontalPositioningAccuracy = messageArr[12];
                        message.Altitude = messageArr[13];
                        message.Mileage = messageArr[14];
                        message.RunTime = messageArr[15];
                        message.BaseStationInfo = messageArr[16];
                        message.IOPortStatus = messageArr[17];
                        message.AnalogInputValue = messageArr[18];

                        var batteryHex = messageArr[18].Split('|')[3];
                        var voltage = int.Parse(batteryHex, System.Globalization.NumberStyles.HexNumber); ;
                        var battery = ((voltage - 2114)*100)/492;
                        message.Battery = battery;
                    }  else if (message.CommandType == "AFF")
                    {
                        message.EventCode = messageArr[4];
                        message.Latitude = messageArr[5];
                        message.Longitude = messageArr[6];

                        string formatString = "yy''MM''dd''HH''mm''ss";
                        string sampleData = messageArr[7]; ;
                        message.DateAndTime = DateTime.ParseExact(sampleData, formatString, null);
                        message.PositioningStatus = messageArr[8];
                        message.NumberOfSatellites = Convert.ToInt32(messageArr[9]);
                        message.GsmSignalStrength = Convert.ToInt32(messageArr[10]);
                        message.Speed = messageArr[11];
                        message.Direction = Convert.ToInt32(messageArr[12]);
                        message.HorizontalPositioningAccuracy = messageArr[13];
                        message.Altitude = messageArr[14];
                        message.Mileage = messageArr[15];
                        message.RunTime = messageArr[16];
                        message.BaseStationInfo = messageArr[17];
                        message.IOPortStatus = messageArr[18];
                        message.AnalogInputValue = messageArr[19];

                        var batteryHex = messageArr[19].Split('|')[3];
                        var voltage = int.Parse(batteryHex, System.Globalization.NumberStyles.HexNumber); ;
                        var battery = ((voltage - 2114) * 100) / 492;
                        message.Battery = battery;
                    }
                    else
                    {
                    }
                }

            }



            return message;
        }


    }
}
