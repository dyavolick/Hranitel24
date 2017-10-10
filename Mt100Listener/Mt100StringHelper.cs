using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mt100Listener
{
    public class Mt100StringHelper
    {
        public Mt100Message GetMt100Message(string s)
        {
            var message = new Mt100Message();
            //"$MGV002,860719021429240,,R,240415,001631,V,5241.51296,N,02522.36540,E,00,00,00,0.97,0.069,,157.6,,257,02,007D,1CFD,21,,,,,,,,,00,100,Sos;!"
            if (!String.IsNullOrEmpty(s) && s.IndexOf(',') > -1)
            {
                var messageArr = s.Split(',');
                if (messageArr.Length > 3 && messageArr[0].StartsWith("STX"))
                {
                    if (messageArr[2].StartsWith("$GPRMC"))
                    {
                        message.Imei = messageArr[17].Remove(0, (messageArr[17].IndexOf(':')+1));
                       //message.DeviceName = messageArr[2]; 
                        message.PositioningStatus = messageArr[15]; // F или L
                        string formatString = "dd''MM''yy''HH''mm''ss";
                        var sampleData = messageArr[11] + messageArr[3].Remove(messageArr[3].IndexOf('.'));
                        //  System date, format: DDMMYY.  System time, format: HHMMSS.
                        message.DateAndTime = DateTime.ParseExact(sampleData, formatString, null);
                        //message.GPSFix = messageArr[6]; //‘A’ means GPS fix successfully, ‘V’ means GPS can not fix.
                        var latitude = messageArr[5].Replace('.', ',');
                            // Latitude (degrees & minutes), format: DDMM.MMMM.
                        var indicatorNorthSouth = messageArr[6];
                        var longitude = messageArr[7].Replace('.', ',');
                        //Longitude (degrees & minutes), format: DDDMM.MMMMM.
                        var indicatorEastWest = messageArr[8];
                        message.Latitude =
                            ((indicatorNorthSouth == "S" ? -1 : 1)*
                             Math.Round(Convert.ToDecimal(latitude.Remove(2), new CultureInfo("ru-RU")) +
                                        (Convert.ToDecimal(latitude.Remove(0, 2), new CultureInfo("ru-RU"))/60), 6))
                                .ToString().Replace(',', '.');
                        message.Longitude =
                            ((indicatorEastWest == "W" ? -1 : 1)*
                             Math.Round(Convert.ToDecimal(longitude.Remove(3), new CultureInfo("ru-RU")) +
                                        (Convert.ToDecimal(longitude.Remove(0, 3), new CultureInfo("ru-RU"))/60), 6))
                                .ToString().Replace(',', '.');

                        var batteryTmp = messageArr[20].Remove(0, messageArr[20].IndexOf('=') + 1);

                        message.Battery = Convert.ToInt32(batteryTmp.Remove(batteryTmp.IndexOf('%')));
                        message.EventCode = messageArr[16];
                        Console.WriteLine(message.EventCode);

                    }
                }

            }
            return message;
        }
    }
}
