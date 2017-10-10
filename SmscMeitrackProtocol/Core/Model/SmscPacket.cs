using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmscMeitrackProtocol.Core.Model
{
    public class SmscPacket
    {
        public int id { get; set; } //20663135,
        public DateTime received { get; set; }// "19.01.2015 23:59:06",
        public string phone { get; set; }  //"79091909837",
        public string message { get; set; }  //"Quiet,011915 20:58,A,23,0Km/h,68%,http://maps.google.com/maps?f=q&hl=en&q=53.187313,50.116085&ie=UTF8&z=16&iwloc=addr&om=1",
        public string to_phone { get; set; } // "79023500131",
        public DateTime sent { get; set; } // "19.01.2015 23:59:05"
        public string error { get; set; }//"описание",
        public int error_code { get; set; }//N
    }
}

