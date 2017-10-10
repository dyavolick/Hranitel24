using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Helpers;

namespace Mt70Listener
{
    public class Mt70LogRepository       
    {
        //private readonly string _connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectLogStr"].ToString()].ToString();

        public void AddLog(string message)
        {
            (new FileHelper()).SaveMessage("log.txt", message);
        }
    }
}
