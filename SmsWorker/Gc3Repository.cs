using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Helpers;
using BusinessObject.Models;
using Dapper;

namespace SmsWorker
{
    public class Gc3Repository
    {
        private readonly string _connectionString = ConfigHelper.GetConfigValueByKey(ConfigHelper.GetConfigValueByKey("ConnectStr"),"",true);

        public DeviceModel SaveMessage(Gc3Message message)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var result = conn.Query<DeviceModel>(@"Gc3AddMessage",
               new
               {
                   Protocol = 1,
                   message.Phone,
                   message.Latitude,
                   message.Longitude,
                   message.DateAndTime,
                   message.PositioningStatus,
                   message.Speed,
                   message.Battery,
                   message.Type,
               }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return result;

            }
        }
        public DeviceModel GetDeviceByPhone(string phone)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var result = conn.Query<DeviceModel>(@"Gc3GetPhone",
               new
               {
                   phone
               }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return result ?? new DeviceModel();

            }
        }

          public void SosClose(int deviceId, int sosclose)
          {
              using (var conn = new SqlConnection(_connectionString))
              {
                  conn.Open();
                  var result = conn.Execute(@"Gc3SosClose",
                 new
                 {
                     DeviceId = deviceId,
                     SosClose =sosclose
                 }, commandType: CommandType.StoredProcedure);   

              }
          }

    }
}
