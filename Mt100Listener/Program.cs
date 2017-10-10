using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Helpers;

namespace Mt100Listener
{
    class Program
    {
        static void Main(string[] args)
        {
            StartServerUdp();
        }

        public static void StartServerUdp()
        {
            try
            {
                var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
                (new UdpNoAsyncServer(port)).Starter();
            }
            catch (Exception e)
            {
                (new FileHelper()).SaveMessage("error.txt", e.ToString());
                StartServerUdp();
            }
        }
    }
}
