using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using BusinessObject.Helpers;
using BusinessObject.Models;
using Microsoft.AspNet.SignalR.Client;

namespace UdpListener.Mt90
{
    public class Mt90UdpNoAsyncServer
    {
        private Mt90Repository repository = new Mt90Repository();
        private int portReceive { get; set; }

        public Mt90UdpNoAsyncServer(int port)
        {
            portReceive = port;
        }

        public void Starter()
        {
            Console.WriteLine("*** Udp ServerNoAsync Started {0} *** ", DateTime.Now.ToString("G"));
            var ip = IPAddress.Any;
            Console.WriteLine("Listening on : {0}:{1}", ip.ToString(), portReceive);
            bool done = false;

            UdpClient listener = new UdpClient(portReceive);
            IPEndPoint groupEP = new IPEndPoint(ip, portReceive);
            var stringHelper = new Mt90StringHelper();


            while (!done)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                var aOut = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), aOut);

                // var message = stringHelper.GetMt90Message(aOut);
                //  message.Ip = groupEP.ToString();

                //   Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), aOut);
                //if (message.CommandType == "AFF")
                //{
                //    var l = ("," + message.Imei + ",AFF,1*").Length;
                //    var s = "@@" + message.DataIdentifier + "" + (l + 4) + "," + message.Imei + ",AFF,1*";
                //    var checksums = stringHelper.GetChecksums(s);
                //    s += checksums;
                //    s += "\r\n";

                //    Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), s);
                //    var datagram = Encoding.ASCII.GetBytes(s);
                //    listener.Send(datagram, datagram.Length, groupEP);
                //}
                //Task.Run(() => SendWebClients(message));
            }
        }


        private void SendWebClients(Mt90Message message)
        {


            var device = repository.SaveMessage(message);
            if (device != null)
            {
                if (message.EventCode == "1")
                {
                    try
                    {
                        var messageSms = HttpUtility.UrlEncode("На " + device.Name + " сработал SOS. https://maps.google.com/maps?q=" + device.LastCoordinates, Encoding.UTF8);
                        Addressee(device, messageSms);

                    }
                    catch (Exception e)
                    {
                        (new FileHelper()).SaveMessage("error.txt", e.ToString());
                    }
                }

                if (message.EventCode == "21")
                {
                    try
                    {
                        var messageSms = HttpUtility.UrlEncode(device.Name + " покинуло гео-зону. https://maps.google.com/maps?q=" + device.LastCoordinates, Encoding.UTF8);
                        Addressee(device, messageSms);
                    }
                    catch (Exception e)
                    {
                        (new FileHelper()).SaveMessage("error.txt", e.ToString());
                    }
                }
                if (message.EventCode == "17")
                {
                    try
                    {
                        var messageSms = HttpUtility.UrlEncode(device.Name + " села батарейка. https://maps.google.com/maps?q=" + device.LastCoordinates, Encoding.UTF8);
                        var serviceBus = new ServiceBusHelper();
                        if (!String.IsNullOrEmpty(device.Addressee1) && device.Addressee1LowBattery)
                        {
                            var smsSender = new SmsSenderModel()
                            {
                                Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
                                Phone = device.Addressee1,
                                Message = messageSms,
                                CrateDate = DateTime.UtcNow
                            };

                            var json = new JavaScriptSerializer().Serialize(smsSender);
                            serviceBus.SendMessages("SmsSender", json);
                        }
                        if (!String.IsNullOrEmpty(device.Addressee2) && device.Addressee2LowBattery)
                        {
                            var smsSender = new SmsSenderModel()
                            {
                                Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
                                Phone = device.Addressee2,
                                Message = messageSms,
                                CrateDate = DateTime.UtcNow
                            };

                            var json = new JavaScriptSerializer().Serialize(smsSender);
                            serviceBus.SendMessages("SmsSender", json);
                        }
                        if (!String.IsNullOrEmpty(device.Addressee3) && device.Addressee3LowBattery)
                        {
                            var smsSender = new SmsSenderModel()
                            {
                                Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
                                Phone = device.Addressee3,
                                Message = messageSms,
                                CrateDate = DateTime.UtcNow
                            };

                            var json = new JavaScriptSerializer().Serialize(smsSender);
                            serviceBus.SendMessages("SmsSender", json);
                        }
                        if (!String.IsNullOrEmpty(device.Addressee4) && device.Addressee4LowBattery)
                        {
                            var smsSender = new SmsSenderModel()
                            {
                                Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
                                Phone = device.Addressee4,
                                Message = messageSms,
                                CrateDate = DateTime.UtcNow
                            };

                            var json = new JavaScriptSerializer().Serialize(smsSender);
                            serviceBus.SendMessages("SmsSender", json);
                        }
                    }
                    catch (Exception e)
                    {
                        (new FileHelper()).SaveMessage("error.txt", e.ToString());
                    }
                }
            }

            try
            {
                using (var hubConnection = new HubConnection("http://cb.hranitel24.ru:80"))
                {
                    var hubProxy = hubConnection.CreateHubProxy("deviceHub");
                    hubConnection.Start().Wait();
                    hubProxy.Invoke("UpdateDevice", (new JavaScriptSerializer()).Serialize(device));
                }
            }
            catch (Exception e)
            {
                (new FileHelper()).SaveMessage("error.txt", e.ToString());
            }

            try
            {
                using (var hubConnection = new HubConnection("http://crm.hranitel24.ru:80"))
                {
                    var hubProxy = hubConnection.CreateHubProxy("deviceHub");
                    hubConnection.Start().Wait();
                    hubProxy.Invoke("UpdateDevice", (new JavaScriptSerializer()).Serialize(device));
                }
            }
            catch (Exception e)
            {
                (new FileHelper()).SaveMessage("error.txt", e.ToString());
            }


        }

        public void Addressee(DeviceModel device, string messageSms)
        {
            var serviceBus = new ServiceBusHelper();
            if (!String.IsNullOrEmpty(device.Addressee1))
            {
                var smsSender = new SmsSenderModel()
                {
                    Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
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
                    Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
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
                    Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
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
                    Sender = ConfigurationManager.AppSettings["SmsSender"].ToString(),
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
