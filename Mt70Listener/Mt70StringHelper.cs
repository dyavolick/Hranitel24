using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mt70Listener
{
    public class Mt70StringHelper
    {
        public Mt70Message GetMt70Message(string s)
        {
            var message = new Mt70Message();
            //"$MGV002,860719021429240,,R,240415,001631,V,5241.51296,N,02522.36540,E,00,00,00,0.97,0.069,,157.6,,257,02,007D,1CFD,21,,,,,,,,,00,100,Sos;!"
            if (!String.IsNullOrEmpty(s) && s.IndexOf(',') > -1)
            {
                var messageArr = s.Split(',');
                if (messageArr.Length > 3 && messageArr[0].StartsWith("$MGV002"))
                {
                    message.Imei = messageArr[1];
                    //message.DeviceName = messageArr[2]; 
                    message.PositioningStatus = messageArr[3]; // R или S
                    string formatString = "dd''MM''yy''HH''mm''ss";
                    var sampleData = messageArr[4] + messageArr[5];
                        //  System date, format: DDMMYY.  System time, format: HHMMSS.
                    message.DateAndTime = DateTime.ParseExact(sampleData, formatString, null);
                    //message.GPSFix = messageArr[6]; //‘A’ means GPS fix successfully, ‘V’ means GPS can not fix.
                    var latitude = messageArr[7].Replace('.', ','); // Latitude (degrees & minutes), format: DDMM.MMMM.
                    var indicatorNorthSouth = messageArr[8];
                    var longitude = messageArr[9].Replace('.', ',');
                        //Longitude (degrees & minutes), format: DDDMM.MMMMM.
                    var indicatorEastWest = messageArr[10];
                    message.Latitude =
                        ((indicatorNorthSouth =="S"?-1:1)*  Math.Round( Convert.ToDecimal(latitude.Remove(2), new CultureInfo("ru-RU")) +
                         (Convert.ToDecimal(latitude.Remove(0, 2), new CultureInfo("ru-RU")) / 60), 6)).ToString().Replace(',', '.');
                    message.Longitude =
                        ((indicatorEastWest == "W" ? -1 : 1) * Math.Round(Convert.ToDecimal(longitude.Remove(3), new CultureInfo("ru-RU")) +
                         (Convert.ToDecimal(longitude.Remove(0, 3), new CultureInfo("ru-RU")) / 60), 6)).ToString().Replace(',', '.');

                    message.Battery = Convert.ToInt32(messageArr[33]);
                    message.EventCode = messageArr[34].Remove(messageArr[34].IndexOf(';'));


                }

            }



            return message;
        }
    }
}
