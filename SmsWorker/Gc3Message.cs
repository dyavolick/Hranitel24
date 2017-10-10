using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsWorker
{
    public class Gc3Message
    {
        public string Phone { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime DateAndTime { get; set; }
        public string PositioningStatus { get; set; }
        public string Speed { get; set; }
        public int Battery { get; set; }
        public int Type { get; set; }
    }
}
