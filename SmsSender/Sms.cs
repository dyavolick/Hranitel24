using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSender
{
  public  class Sms
    {
        public int Id { get; set; }
        public int AnswerType { get; set; }
        public int DeviceId { get; set; }
        public int SmsId { get; set; }
        public string Coordinates { get; set; }
        public int Battery { get; set; }
        public int Speed { get; set; }
        public DateTime Sent { get; set; }
        public DateTime Received { get; set; }
        public string PhoneTo { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; }
        public int Error_code { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
