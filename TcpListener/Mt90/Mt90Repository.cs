﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace TcpListener.Mt90
{
    public class Mt90Repository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConnectStr"].ToString()].ToString();

        public int SaveMessage(Mt90Message message)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var result = conn.Query<int>(@"Mt90AddMessage",
               new
               {
                   Protocol =1,
                   message.DataIdentifier,
                   message.DataLength,
                   message.CommandType,
                   message.Imei,
                   message.EventCode,
                   message.Latitude,
                   message.Longitude,
                   message.DateAndTime,
                   message.PositioningStatus,
                   message.NumberOfSatellites,
                   message.GsmSignalStrength,
                   message.Speed,
                   message.Direction,
                   message.HorizontalPositioningAccuracy,
                   message.Altitude,
                   message.Mileage,
                   message.RunTime,
                   message.BaseStationInfo,
                   message.IOPortStatus,
                   message.AnalogInputValue ,
                   message.Ip,
                   message.Battery,
               }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return result;
            }
        }

    }
}
