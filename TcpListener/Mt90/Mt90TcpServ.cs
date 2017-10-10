using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TcpListener.Mt90
{
  public  class Mt90TcpServ
  {
      private ArrayList m_aryClients = new ArrayList();	// Список подключенных клиентов
      private Mt90Repository repository =new Mt90Repository();

      private bool a = true;

      public void Start(int port)
      {
          Console.WriteLine("*** Mt 90 Tcp Server Started {0} *** ", DateTime.Now.ToString("G"));
          var ip = IPAddress.Any;
          Console.WriteLine("Listening on : {0}:{1}", ip.ToString(), port);
          Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          listener.Bind(new IPEndPoint(ip, port));

          listener.Listen(1000);
          listener.BeginAccept(new AsyncCallback(this.OnConnectRequest), listener);
          Console.WriteLine("Press Enter to exit");
          Console.ReadLine();
          Console.WriteLine("OK that does it! Screw you guys I'm going home...");
          listener.Close();
          GC.Collect();
          GC.WaitForPendingFinalizers();
      }

      /// <summary>
      /// Принимаем подключения нового клиента и отправляем его в метод обработки новых клиентов. 
      /// </summary>
      /// <param name="ar"></param>
      public void OnConnectRequest(IAsyncResult ar)
      {
          Socket listener = (Socket)ar.AsyncState;
          NewConnection(listener.EndAccept(ar));
          listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
      }

      /// <summary>
      /// Добавляем в список нового клиента
      /// Настраиваем получение с него данных
      /// </summary>
      /// <param name="sockClient">Удерживаемое соединение</param>
      //public void NewConnection( TcpListener listener )
      public void NewConnection(Socket sockClient)
      {
          // Program blocks on Accept() until a client connects.
          //SocketChatClient client = new SocketChatClient( listener.AcceptSocket() );
          var client = new SocketClient(sockClient);
          m_aryClients.Add(client);
          Console.WriteLine("Client {0}, joined", client.Sock.RemoteEndPoint);

          #region Приветствие клиента
          // Get current date and time.
          //DateTime now = DateTime.Now;
          //String strDateLine = "Welcome " + now.ToString("G") + "\n\r";

          // Convert to byte array and send.
          //var s = //"@@Q25,353358017784062,A10*6A\r\n"; 
          //               "@@W25,863158020721157,E91*77\r\n";
          //Byte[] byteDateLine = System.Text.Encoding.ASCII.GetBytes( s.ToCharArray() );
          //client.Sock.Send( byteDateLine, byteDateLine.Length, 0 );
          #endregion

          client.SetupRecieveCallback(this);
      }

      /// <summary>
      /// Получение данных от клиента. 
      /// Примечание: Если не данные получили соединение, вероятно, умерло.
      /// </summary>
      /// <param name="ar"></param>
      public void OnRecievedData(IAsyncResult ar)
      {
          var client = (SocketClient)ar.AsyncState;
          byte[] aryRet = client.GetRecievedData(ar);
          var stringHelper = new Mt90StringHelper();

          // Если данные не получил то связь, вероятно, умер
          if (aryRet.Length < 1)
          {
              (new EventCreator()).Create(client.Sock.RemoteEndPoint.ToString(), "disconnected");
              Console.WriteLine("Client {0}, disconnected", client.Sock.RemoteEndPoint);
              client.Sock.Close();
              m_aryClients.Remove(client);
              return;
          }
          else
          {
        
              var aOut = Encoding.ASCII.GetString(aryRet, 0, aryRet.Length);
              var message = stringHelper.GetMt90Message(aOut);
              message.Ip = client.Sock.RemoteEndPoint.ToString();
              repository.SaveMessage(message);
              (new EventCreator()).Create(client.Sock.RemoteEndPoint.ToString(), aOut);
              //Console.WriteLine("{0}==={1};", client.Sock.RemoteEndPoint, aOut);
          }

          //if (a)
          //{
 
          //try
          //    {

          //        var s = //"@@Q25,353358017784062,A10*6A\r\n"; 
          //                  "@@W25,863158020721157,E91*77\r\n";
          //        Byte[] byteDateLine =Encoding.ASCII.GetBytes(s.ToCharArray());
          //        client.Sock.Send(byteDateLine, byteDateLine.Length, 0);
          //        Console.WriteLine("Sended to client {0} === {1}", client.Sock.RemoteEndPoint, s);
                
          //    }
          //    catch
          //    {
          //        // If the send fails the close the connection
          //        Console.WriteLine("Send to client {0} failed", client.Sock.RemoteEndPoint);
          //        client.Sock.Close();
          //        m_aryClients.Remove(client);
          //        return;
          //    }
          //    a = false;
          //}
          


         
          //    //Console.WriteLine((s).Length);

          //    //Console.WriteLine("Checksums: " + (new Checksums()).GetChecksums("$$B145,863158020721157,AAA,35,52.691971,25.372790,150204170317,V,0,8,0,0,0.0,0,7067,150931,257|1|00FE|066A,0000,0000|0000|0000|09E9|0000,00000001,*"));



          // Send the recieved data to all clients (including sender for echo)
          //Отправьте получил данные для всех клиентов (в том числе отправителем эхо)
          //foreach (SocketClient clientSend in m_aryClients)
          //{
          //    try
          //    {
          //        clientSend.Sock.Send(aryRet);
          //    }
          //    catch
          //    {
          //        // If the send fails the close the connection
          //        Console.WriteLine("Send to client {0} failed", client.Sock.RemoteEndPoint);
          //        clientSend.Sock.Close();
          //        m_aryClients.Remove(client);
          //        return;
          //    }
          //}
          client.SetupRecieveCallback(this);
      }

       public byte[] GetBytes(string str)
      {
          byte[] bytes = new byte[str.Length * sizeof(char)];
          System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
          return bytes;
      }
  }

  internal class SocketClient
  {
      #region System
      private Socket m_sock; // Подключенный клиент
      private byte[] m_byBuff = new byte[1024]; // Буфер данных
      /// <summary>
      /// Конструктор
      /// </summary>
      /// <param name="sock">сокет клиента</param>
      public SocketClient(Socket sock)
      {
          m_sock = sock;
      }

      // Подключенный клиент
      public Socket Sock
      {
          get { return m_sock; }
      }

     
      #endregion

      /// <summary>
      /// Настройка
      /// </summary>
      /// <param name="app"></param>
      public void SetupRecieveCallback(Mt90TcpServ app)
      {
          try
          {
              AsyncCallback recieveData = new AsyncCallback(app.OnRecievedData);
              m_sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, this);
          }
          catch (Exception ex)
          {
              Console.WriteLine("Recieve callback setup failed! {0}", ex.Message);
          }
      }

      /// <summary>
      /// Получение данных с клиента.
      /// </summary>
      /// <param name="ar"></param>
      /// <returns>Массив байтов, содержащих полученную информацию</returns>
      public byte[] GetRecievedData(IAsyncResult ar)
      {
          int nBytesRec = 0;
          try
          {
              nBytesRec = m_sock.EndReceive(ar);
          }
          catch { }
          byte[] byReturn = new byte[nBytesRec];
          Array.Copy(m_byBuff, byReturn, nBytesRec);

          //// Проверяет нет ли непрочитанных данных
          //int nToBeRead = m_sock.Available;
          //if (nToBeRead > 0)
          //{
          //    byte[] byData = new byte[nToBeRead];
          //    m_sock.Receive(byData);

          //    // Append byData to byReturn here
          //}

          return byReturn;
      }

  }
}
