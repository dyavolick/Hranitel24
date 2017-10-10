using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpListener
{
  public  class EventCreator
    {
        public void Create(string ip, string message)
        {
            Console.WriteLine("Received broadcast from {0} :\n {1}\n", ip, message);

            StreamWriter sw = new StreamWriter("ouput.txt", true);
            sw.WriteLine("{0}==={1}", ip, message);
            sw.Close();
        }
    }
}
