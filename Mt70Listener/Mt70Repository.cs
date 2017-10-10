using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Models;
using Dapper;

namespace Mt70Listener
{
    public class Mt70Repository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectStr"].ToString()].ToString();

        public DeviceModel SaveMessage(Mt70Message message)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var result = conn.Query<DeviceModel>(@"Mt70AddMessage",
               new
               {
                   Protocol = 2,
                   message.Imei,
                   message.EventCode,
                   message.Latitude,
                   message.Longitude,
                   message.DateAndTime,
                   message.PositioningStatus,
                   message.Ip,
                   message.Battery,
               }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return result ?? new DeviceModel();
            }
        }

        public DeviceModel GetDeviceByImei(string imei)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var result = conn.Query<DeviceModel>(@"DeviceGetByImei",
               new
               {
                   imei
               }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return result ?? new DeviceModel();
            }
        }

    }
}
