using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Helpers;
using UdpListener.Mt90;

namespace UdpListener
{
    class Program
    {
        static void Main(string[] args)
        {

            //var l = ",353358017784062,AFF,1*".Length;
            //var s = "@@h" + (l + 4) + ",353358017784062,AFF,1*";
            //var checksums =(new Mt90StringHelper()).GetChecksums(s);
            //s += checksums;
            //s += "\r\n";
            //              Console.WriteLine(s);
            //Console.ReadLine();
            StartServerUdp();
        }

        public static void StartServerUdp()
        {
            try
            {
                var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
                (new Mt90UdpNoAsyncServer(port)).Starter();
            }
            catch (Exception e)
            {
                (new FileHelper()).SaveMessage("error.txt", e.ToString());
                StartServerUdp();
            }
        }
    }
}
