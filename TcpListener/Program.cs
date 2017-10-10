using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TcpListener.Mt90;

namespace TcpListener
{
    class Program
    {
        static void Main(string[] args)
        {    
            StartServerMt90Tcp(); 
            //var s = "$$B145,863158020721157,AAA,35,52.691971,25.372790,150204170317,V,0,8,0,0,0.0,0,7067,150931,257|1|00FE|066A,0000,0000|0000|0000|09E9|0000,00000001,*";
            //var stringHelper = new Mt90StringHelper();

            //Console.WriteLine(stringHelper.GetMt90Message(s).GsmSignalStrength);  
        }

        public static void StartServerMt90Tcp()
        {
            try
            {
                var port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"].ToString());
                (new Mt90TcpServ()).Start(port);
            }
            catch
            {
                StreamWriter sw = new StreamWriter("error.txt", true);
                sw.WriteLine("{0}==={1}", DateTime.UtcNow, "Server Error");
                sw.Close();
                StartServerMt90Tcp();
            }
        }
    }
}
