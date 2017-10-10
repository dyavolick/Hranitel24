



using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using Apache.NMS;
using Apache.NMS.Util;
using BusinessObject.Helpers;
using BusinessObject.Models;

namespace SmsSender
{

    class Program
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
                IConnection connection = factory.CreateConnection();
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
                    var smsJson = (string) objectMessage.Body;
                    if (!String.IsNullOrEmpty(smsJson))
                    {
                        var sUserName = ConfigHelper.GetConfigValueByKey("smsUserName");
                        var sPwd = ConfigHelper.GetConfigValueByKey("smsPwd");
                        Console.WriteLine(string.Format("Message received: Body = {0}", smsJson));
                        var sms = new JavaScriptSerializer().Deserialize<SmsSenderModel>(smsJson);
                        var url =
                            String.Format(
                                "http://smsc.ru/sys/send.php?login={0}&psw={1}&phones={2}&mes={3}&sender={4}&charset=utf-8&translit={5}",
                                sUserName, sPwd, sms.Phone, sms.Message, sms.Sender,
                                ConfigHelper.GetConfigValueByKey("Translit"));
                        var request = WebRequest.Create(url);
                        request.Credentials = CredentialCache.DefaultCredentials;
                        var response = (HttpWebResponse) request.GetResponse();
                        response.Close();
                        Console.WriteLine(url);
                        message.Acknowledge();
                        Console.WriteLine(smsJson);

                        StreamWriter sw = new StreamWriter("out.txt", true);
                        sw.WriteLine("Message==={0}==={1}", DateTime.Now.ToString(), smsJson);
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
}

