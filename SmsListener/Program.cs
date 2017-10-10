using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using Apache.NMS;
using Apache.NMS.Util;
using BusinessObject.Enum;
using BusinessObject.Helpers;
using BusinessObject.Models;


namespace SmsListener
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
            Console.WriteLine("Start message wait");
            while (true)
            {
                try
                {
                    // Определим нужное максимальное количество потоков
                    // Пусть будет по 4 на каждый процессор
                    int MaxThreadsCount = Environment.ProcessorCount * 4;
                    // Установим максимальное количество рабочих потоков
                    ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                    // Установим минимальное количество рабочих потоков
                    ThreadPool.SetMinThreads(2, 2);
                    new Server(Convert.ToInt32(ConfigHelper.GetConfigValueByKey("ServerPort")));
                }
                catch (Exception e)
                {
                    StreamWriter sw = new StreamWriter("error.txt", true);
                    sw.WriteLine("Error==={0}==={1}", DateTime.Now.ToString(), e.ToString());
                    sw.Close();
                }
                Thread.Sleep(5000);
            }
        }
    }

    // Класс-обработчик клиента
    class Client
    {
       // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public Client(TcpClient Client)
        {
            // Объявим строку, в которой будет хранится запрос клиента
            string Request = "";
            // Буфер для хранения принятых от клиента данных
            byte[] Buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int Count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                // Преобразуем эти данные в строку и добавим ее к переменной Request
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                // Запрос должен обрываться последовательностью \r\n\r\n
                // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
                // по идее не должен быть больше 4 килобайт
                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                {
                    break;
                }
            }

            string Html = "<html><body><h1>Ok</h1></body></html>";
            // Посылаем заголовки
            string Headers = "HTTP/1.1 200 OK\nContent-Type: text/html\nContent-Length: " + Html.Length.ToString() +
                             "\n\n" + Html;
            byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
            Client.GetStream().Write(HeadersBuffer, 0, HeadersBuffer.Length);
            Client.Close();  

            if (!String.IsNullOrEmpty(Request))
            {
                Console.WriteLine(Request);
                var ind = Request.IndexOf("phone");
                if (ind > 0)
                {
                    var aOut = Request.Remove(0, ind);
                    if (!String.IsNullOrEmpty(aOut) && aOut.IndexOf('&') > -1)
                    {
                        Console.WriteLine(aOut);
                        var serviceBus = new ServiceBusHelper();
                        serviceBus.SendMessages(ConfigHelper.GetConfigValueByKey("QueueName"), aOut);
                    }
                }
            }
        }
    }

    internal class Server
    {
        private TcpListener Listener; // Объект, принимающий TCP-клиентов

        // Запуск сервера
        public Server(int Port)
        {
            Listener = new TcpListener(IPAddress.Any, Port); // Создаем "слушателя" для указанного порта
            Listener.Start(); // Запускаем его

            // В бесконечном цикле
            while (true)
            {
                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());

                /*
                // Принимаем нового клиента
                TcpClient Client = Listener.AcceptTcpClient();
                // Создаем поток
                Thread Thread = new Thread(new ParameterizedThreadStart(ClientThread));
                // И запускаем этот поток, передавая ему принятого клиента
                Thread.Start(Client);
                */
            }
        }

        private static void ClientThread(Object StateInfo)
        {
            // Просто создаем новый экземпляр класса Client и передаем ему приведенный к классу TcpClient объект StateInfo
            new Client((TcpClient)StateInfo);
        }

        // Остановка сервера
        ~Server()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
            }
        }

    }
}


