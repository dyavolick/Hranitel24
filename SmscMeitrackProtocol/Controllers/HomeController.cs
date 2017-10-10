using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hranitel24.Data.Abstract;
using Hranitel24.Data.Entites;
using SmscMeitrackProtocol.Core.Enum;
using SmscMeitrackProtocol.Core.Model;

namespace SmscMeitrackProtocol.Controllers
{
    public class HomeController : Controller
    {

        #region System
        internal readonly IDeviceService deviceService;
        internal readonly ILogService logService;
        internal readonly IPaymentsService paymentsService;
        internal readonly IUserService userService;
        internal readonly IGeneralService generalService;
        internal readonly ISmsProtocolService smsProtocolService;

        public HomeController(IDeviceService deviceService, ILogService logService, IPaymentsService paymentsService, IUserService userService, IGeneralService generalService, ISmsProtocolService smsProtocolService)
        {
            this.deviceService = deviceService;
            this.logService = logService;
            this.paymentsService = paymentsService;
            this.userService = userService;
            this.generalService = generalService;
            this.smsProtocolService = smsProtocolService;
        }
        #endregion
        //[HttpGet]
        //public ActionResult Index()
        //{

        //    return View();
        //}
        //  [HttpPost]
        public ActionResult Index(SmscInputPacket smscInputPacket)
        {
            //SmscInputPacket smscInputPacket = new SmscInputPacket()
            // ;

            string smscInputPacketJson = "";

            var sms = new Sms();

            var device = deviceService.GetDeviceByPhone(smscInputPacket.phone);
            if (device != null)
                sms.DeviceId = device.Id;

            // var smsMessage = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<SmscPacket>(smscInputPacket.mes);
            smscInputPacketJson = smscInputPacket.mes; //	Текст SMS-сообщения.
            if (!String.IsNullOrEmpty(smscInputPacket.mes))
            {
                var mesArray = smscInputPacket.mes.Split(',');
                if (mesArray[0] != null)
                {
                    var answerTypeString = mesArray[0];
                    switch (answerTypeString)
                    {


                        case "SOS":
                            sms.AnswerType = (int)AnswerType.SOS;
                            sms.Status = 1;

                            try
                            {
                                var coordinatesUrl = mesArray[6] + "," + mesArray[7];
                                var s1 = coordinatesUrl.Remove(0, coordinatesUrl.LastIndexOf("q=") + 2);
                                var s2 = s1.Remove(s1.IndexOf("&ie"));
                                sms.Coordinates = s2;
                            }
                            catch
                            {
                            }

                            try
                            {
                                var s1 = mesArray[4].Remove(mesArray[4].IndexOf("Km/h"));
                                sms.Speed = Int32.Parse(s1);
                            }
                            catch
                            {
                            }

                            try
                            {
                                var s1 = mesArray[5].Remove(mesArray[5].IndexOf("%"));
                                sms.Battery = Int32.Parse(s1);
                            }
                            catch
                            {
                            }
                            break;
                    }
                }

            }     

            sms.SmsId = smscInputPacket.id;//	Уникальный идентификатор входящего сообщения, назначаемый Сервером автоматически.
            sms.Phone = smscInputPacket.phone; //	Номер телефона абонента.

            sms.PhoneTo = smscInputPacket.to; //	Входящий номер телефона, на который было отправлено сообщение абонентом.
            sms.Sent = smscInputPacket.sent ?? DateTime.Now; //	Время отправки сообщения абонентом в виде штампа в секундах.
            sms.Received = smscInputPacket.time ?? DateTime.Now;

            smsProtocolService.AddSms(sms);
            //.Received.ToString("MM/dd/yyyy hh:mm:ss tt")
            var put = Server.MapPath("~") + "log.txt";
            using (StreamWriter w = System.IO.File.AppendText(put))
            {
                Log(smscInputPacketJson, w);
            }

            return View();
        }



        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
    }
}