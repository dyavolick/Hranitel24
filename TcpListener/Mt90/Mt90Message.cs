using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpListener.Mt90
{
    public class Mt90Message
    {
        public string Ip { get; set; }
        public int Protocol { get; set; }
        public string DataIdentifier { get; set; }//Флаг
        public int DataLength { get; set; } //Длинна данных
        public string CommandType { get; set; }
        public string Imei { get; set; }
        public string EventCode { get; set; } //Код сообщения
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DateAndTime { get; set; }
        public string PositioningStatus { get; set; }//A (Valid) или V (Invalid)
        public int NumberOfSatellites { get; set; } //Доступных спутников
        public int GsmSignalStrength { get; set; }      //Сила сигнала GSM
        public string Speed { get; set; }
        public int Direction { get; set; }   // Направление (0 - Север) от 0 до 359
        public string HorizontalPositioningAccuracy { get; set; } //Точность позиционирования ()
        public string Altitude { get; set; }//Высота
        public string Mileage { get; set; }//пробег кнопки
        public string RunTime { get; set; }//время работы
        public string BaseStationInfo { get; set; } //Базовая станция
        public string IOPortStatus { get; set; } // Hexadecimal
        public string AnalogInputValue { get; set; }
        public int Battery { get; set; }
    }
}
