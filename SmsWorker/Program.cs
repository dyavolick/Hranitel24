using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Apache.NMS;
using Apache.NMS.Util;
using BusinessObject.Enum;
using BusinessObject.Helpers;
using BusinessObject.Models;
using Microsoft.AspNet.SignalR.Client;
using IConnection = Microsoft.AspNet.SignalR.Client.IConnection;

namespace SmsWorker
{
    public class Program
    {

        static void Main(string[] args)
        {

            StartListener();
            Console.WriteLine("Close message wait");
            Console.ReadLine();
        }


        private static void StartListener()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Start message wait");
                    ReceiveMessages();
                }
                catch
                {
                }
                Thread.Sleep(5000);
            }
        }

        private static void ReceiveMessages()
        {       
            try
            {
                IConnectionFactory factory = new NMSConnectionFactory(ConfigHelper.GetConfigValueByKey("QueueConnectionString"));
                Apache.NMS.IConnection connection = factory.CreateConnection();
                //"username", "password"
                connection.Start();
                ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
                IDestination queueDestination = SessionUtil.GetDestination(session, ConfigHelper.GetConfigValueByKey("QueueName"));
                IMessageConsumer consumer = session.CreateConsumer(queueDestination);

                consumer.Listener += new MessageListener(Message_Listener);
                Thread.Sleep(Convert.ToInt32(ConfigHelper.GetConfigValueByKey("TimeOut")));
                connection.Close();

                Console.WriteLine("Restart message wait");

            }
            catch (Exception e)
            {
                StreamWriter sw = new StreamWriter("error.txt", true);
                sw.WriteLine("Error==={0}==={1}", DateTime.Now.ToString(), e.ToString());
                sw.Close();
            }
            ReceiveMessages();
        }

        private static void Message_Listener(IMessage message)
        {
            try
            {
                Console.WriteLine("Add Message");
                IObjectMessage objectMessage = message as IObjectMessage;
                if (objectMessage.Body != null)
                {
                    var sms = (string)objectMessage.Body;
                    if (!String.IsNullOrEmpty(sms))
                    {
                        Console.WriteLine(string.Format("Message = {0}", sms));
                        new Client(sms);
                        StreamWriter sw = new StreamWriter("out.txt", true);
                        sw.WriteLine("Message==={0}==={1}", DateTime.Now.ToString(), sms);
                        sw.Close();
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                StreamWriter sw = new StreamWriter("error.txt", true);
                sw.WriteLine("Error==={0}==={1}", DateTime.Now.ToString(), e.ToString());
                sw.Close();
            }
        }
    }



    // Класс-обработчик клиента
    class Client
    {
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(string aOut)
        {
            var s = aOut;
            Console.WriteLine(aOut);
            if (s.IndexOf('&') > -1)
            {
                var strArr = s.Split('&');
                if (strArr.Length > 0)
                {
                    var phoneStr = "";
                    if (strArr[0].IndexOf("phone=", StringComparison.Ordinal) > -1)
                    {
                        phoneStr = strArr[0].Remove(0, 6);
                    }
                    string phone = Regex.Replace(phoneStr, @"[^0-9]", string.Empty);

                    if (!String.IsNullOrEmpty(phone))
                    {

                        if (strArr[1].IndexOf("mes=", StringComparison.Ordinal) > -1)
                        {
                            (new FileHelper()).SaveMessage("output.txt", "mes==" + strArr[1]);

                            var device = (new Gc3Repository()).GetDeviceByPhone(phone);
                            if (device.Type == DeviceTypeEnum.Gc3)
                            {
                                var mes = strArr[1].Remove(0, 4);
                                var gc3Message = new Gc3Message();
                                Console.WriteLine(mes);
                                gc3Message.Battery = 100;

                                var sendDb = false;

                                if (mes.IndexOf("SOS%20-%20Ende", StringComparison.Ordinal) > -1)
                                {
                                    //SOS Modus ist deaktiviert.

                                    //gc3Message.PositioningStatus = "A";
                                    //sendDb = true;
                                    //gc3Message.Type = 15;
                                    //Console.WriteLine("Position");
                                }
                                else if (mes.IndexOf("SOS%20Modus%20ist%20deaktiviert", StringComparison.Ordinal) >
                                         -1)
                                {
                                    //SOS Modus ist deaktiviert.

                                    //gc3Message.PositioningStatus = "A";
                                    //sendDb = true;
                                    //gc3Message.Type = 15;
                                    //Console.WriteLine("Position");
                                }
                                else if (mes.IndexOf("SOS", StringComparison.Ordinal) > -1)
                                {
                                    var date = DateTime.UtcNow;
                                    var sosEnabled = ((date - device.SosCloseUpdateTime).Minutes > 2);

                                    if (mes.IndexOf("Letzte%20Position", StringComparison.Ordinal) > -1)
                                    {
                                        gc3Message.PositioningStatus = "V";
                                        sendDb = true;

                                        if (!device.SosActive && device.SosClose && sosEnabled)
                                        {
                                            var messageSms =
                                                HttpUtility.UrlEncode(
                                                    "На " + device.Name +
                                                    " сработал SOS. Последние координаты https://maps.google.com/maps?q=" +
                                                    device.LastCoordinates, Encoding.UTF8);
                                            Addressee(device, messageSms);
                                            gc3Message.Type = 1;
                                            (new Gc3Repository()).SosClose(device.Id, 0);
                                        }
                                        else
                                        {
                                            //if ((date - device.SosTime).Minutes > 15)
                                            //{
                                            //var message = "sosaus";
                                            //var smsSender = new SmsSenderRabbitMQ()
                                            //{
                                            //    Sender = ConfigHelper.GetConfigValueByKey("SmsMt90Sender"),
                                            //    Phone = phone, //device.Phone,
                                            //    Message = message,
                                            //    CrateDate = DateTime.UtcNow
                                            //};
                                            //(new Gc3Repository()).SosClose(device.Id, 1);
                                            //var json = new JavaScriptSerializer().Serialize(smsSender);
                                            //var serviceBus = new ServiceBusHelper();
                                            //serviceBus.SendMessages("SmsSender", json);
                                            //}

                                            gc3Message.Type = 2;
                                        }
                                    }
                                    else if (mes.IndexOf("Aktualisierte%20Position", StringComparison.Ordinal) >
                                             -1)
                                    {
                                        gc3Message.Type = !device.SosActive && sosEnabled ? 1 : 2;

                                        var messageSms =
                                            HttpUtility.UrlEncode(
                                                String.Format(
                                                    "Абонент {0} попал в экстренную ситуацию. Его актуальные координаты  https://maps.google.com/maps?q={1}",
                                                    device.Name, device.LastCoordinates), Encoding.UTF8);
                                        Addressee(device, messageSms);

                                        gc3Message.PositioningStatus = "A";
                                        //var message = "sosaus";
                                        //var smsSender = new SmsSenderRabbitMQ()
                                        //{
                                        //    Sender = ConfigHelper.GetConfigValueByKey("SmsMt90Sender"),
                                        //    Phone = phone, //device.Phone,
                                        //    Message = message,
                                        //    CrateDate = DateTime.UtcNow
                                        //};
                                        //(new Gc3Repository()).SosClose(device.Id, 1);
                                        //var json = new JavaScriptSerializer().Serialize(smsSender);
                                        //var serviceBus = new ServiceBusHelper();
                                        //serviceBus.SendMessages("SmsSender", json);
                                        sendDb = true;
                                    }
                                }
                                else if (mes.IndexOf("Akkukapazitaet", StringComparison.Ordinal) > -1)
                                {
                                    gc3Message.PositioningStatus = "A";
                                    sendDb = false;
                                    gc3Message.Type = 15;
                                    Console.WriteLine("Position");
                                }

                                else if (mes.IndexOf("%20ist%0A", StringComparison.Ordinal) > -1)
                                {
                                    gc3Message.PositioningStatus = "A";
                                    sendDb = true;
                                    gc3Message.Type = 15;

                                    var messageSms =
                                        HttpUtility.UrlEncode(
                                            "Актуальные координаты " + device.Name +
                                            " https://maps.google.com/maps?q=" + device.LastCoordinates,
                                            Encoding.UTF8);
                                    Addressee(device, messageSms);

                                    Console.WriteLine("Position");
                                }
                                else if (
                                    mes.IndexOf("Kein%20GPS-Signal%20verfuegbar", StringComparison.Ordinal) >
                                    -1)
                                {
                                    var message = "position";
                                    var smsSender = new SmsSenderModel()
                                    {
                                        Sender = ConfigHelper.GetConfigValueByKey("SmsGc3Sender"),
                                        Phone = phone, //device.Phone,
                                        Message = message,
                                        CrateDate = DateTime.UtcNow
                                    };
                                    var json = new JavaScriptSerializer().Serialize(smsSender);
                                    var serviceBus = new ServiceBusHelper();
                                    serviceBus.SendMessages("SmsSender", json);
                                }

                                var strDecode = HttpUtility.UrlDecode(mes);

                                (new FileHelper()).SaveMessage("output.txt", strDecode);

                                if (sendDb)
                                {
                                    #region  обработка сообщения

                                    var coordinatesIndexStart = mes.IndexOf("geocare%40", StringComparison.Ordinal);
                                    var coordinatesTmp = mes.Remove(0, coordinatesIndexStart + 10);
                                    var coordinatesIndexFinish = coordinatesTmp.IndexOf("%26z%3D",
                                        System.StringComparison.Ordinal);
                                    var coordinates =
                                        HttpUtility.UrlDecode(coordinatesTmp.Remove(coordinatesIndexFinish));

                                    gc3Message.Phone = phone;
                                    gc3Message.Latitude = coordinates.Split(',')[0];
                                    gc3Message.Longitude = coordinates.Split(',')[1];
                                    gc3Message.DateAndTime = DateTime.Now;
                                    gc3Message.Speed = "0";


                                    (new Gc3Repository()).SaveMessage(gc3Message);


                                    //Console.WriteLine(coordinates);
                                    Console.WriteLine(strDecode);

                                    #endregion

                                    var devicea = (new Gc3Repository()).GetDeviceByPhone(phone);
                                    try
                                    {
                                        using (var hubConnection = new HubConnection("http://cb.hranitel24.ru:180"))
                                        {
                                            var hubProxy = hubConnection.CreateHubProxy("deviceHub");
                                            hubConnection.Start().Wait();
                                            hubProxy.Invoke("UpdateDevice",
                                                (new JavaScriptSerializer()).Serialize(devicea));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        (new FileHelper()).SaveMessage("error.txt", "cb==" + e.ToString());
                                    }

                                    try
                                    {
                                        using (var hubConnection = new HubConnection("http://crm.hranitel24.ru:180")
                                            )
                                        {
                                            var hubProxy = hubConnection.CreateHubProxy("deviceHub");
                                            hubConnection.Start().Wait();
                                            hubProxy.Invoke("UpdateDevice",
                                                (new JavaScriptSerializer()).Serialize(devicea));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        (new FileHelper()).SaveMessage("error.txt", "crm==" + e.ToString());

                                    }
                                }
                            }
                        }
                    }
                    //&id=21470454&to=79023500131&time=1427289490&sent=1427289490&smsc= 
                }
            }

        }

        public void Addressee(DeviceModel device, string messageSms)
        {
            var serviceBus = new ServiceBusHelper();
            if (!String.IsNullOrEmpty(device.Addressee1))
            {
                var smsSender = new SmsSenderModel()
                {
                    Sender = ConfigHelper.GetConfigValueByKey("SmsGc3Sender"),
                    Phone = device.Addressee1,
                    Message = messageSms,
                    CrateDate = DateTime.UtcNow
                };

                var json = new JavaScriptSerializer().Serialize(smsSender);
                serviceBus.SendMessages("SmsSender", json);
            }
            if (!String.IsNullOrEmpty(device.Addressee2))
            {
                var smsSender = new SmsSenderModel()
                {
                    Sender = ConfigHelper.GetConfigValueByKey("SmsGc3Sender"),
                    Phone = device.Addressee2,
                    Message = messageSms,
                    CrateDate = DateTime.UtcNow
                };

                var json = new JavaScriptSerializer().Serialize(smsSender);
                serviceBus.SendMessages("SmsSender", json);
            }
            if (!String.IsNullOrEmpty(device.Addressee3))
            {
                var smsSender = new SmsSenderModel()
                {
                    Sender = ConfigHelper.GetConfigValueByKey("SmsGc3Sender"),
                    Phone = device.Addressee3,
                    Message = messageSms,
                    CrateDate = DateTime.UtcNow
                };

                var json = new JavaScriptSerializer().Serialize(smsSender);
                serviceBus.SendMessages("SmsSender", json);
            }
            if (!String.IsNullOrEmpty(device.Addressee4))
            {
                var smsSender = new SmsSenderModel()
                {
                    Sender = ConfigHelper.GetConfigValueByKey("SmsGc3Sender"),
                    Phone = device.Addressee4,
                    Message = messageSms,
                    CrateDate = DateTime.UtcNow
                };

                var json = new JavaScriptSerializer().Serialize(smsSender);
                serviceBus.SendMessages("SmsSender", json);
            }
        }

    }

}


