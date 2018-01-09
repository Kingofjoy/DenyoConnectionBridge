using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Net;

using Denyo.ConnectionBridge.DataStructures;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Denyo.ConnectionBridge.Server.TCPServer
{

    public static class Server
    {
        static TcpListener serverSocket = null;

        static ConcurrentBag<Client> myClients = new ConcurrentBag<Client>();

        static DateTime dtNext;
        static bool bHBInit = false;

        public static bool bAcceptLoop = false;

        public static int ListenerPort { get { return _ListenerPort; } }
        static int _ListenerPort;

        public static bool IsHeartBeatActive = false;
        static int HeartBeatInterval;

        static public bool IsActive { get; set; }

        static public string AppID { get; set; }

        static public string AppType { get; set; }

        static public string AuthToken { get; set; }


        static Server()
        {
           try
            {
                IsActive = false;

                bool.TryParse(ConfigurationManager.AppSettings["IsHeartBeatActive"], out IsHeartBeatActive);

                if (IsHeartBeatActive)
                    HeartBeatInterval = int.Parse(ConfigurationManager.AppSettings["HeartBeatInterval"]);

                AppID = ConfigurationManager.AppSettings["AppID"];
                AppType = ConfigurationManager.AppSettings["AppType"];
                AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                _ListenerPort = int.Parse(ConfigurationManager.AppSettings["SLPort"]);
            }
            catch(Exception ex)
            {

            }
        }

        public static bool Start()
        {
            try
            {
                serverSocket = new TcpListener(IPAddress.Any, ListenerPort);
                serverSocket.Start();

                IsActive = true;
            }
            catch(Exception ex)
            {

            }
            return IsActive;
        }

        public static void AcceptIncomingConnetion()
        {
            Byte[] ByteData;

            try
            {
                while (serverSocket.Pending())
                {
                    Client myClient = new Client();
                    myClient.Instance = serverSocket.AcceptTcpClient();
                    myClient.Stream = myClient.Instance.GetStream();

                    bool bSend, bReceive, bPacketSend, bPacketReceive;
                    bSend = bReceive = bPacketSend = bPacketReceive = false;
                    string data=string.Empty;
                    DataPacket dpTest=new DataPacket();

                    //Send Test
                    #region Init_Send_Test

                    ByteData = Encoding.ASCII.GetBytes("SHB " + DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            if (!myClient.Instance.Connected || !myClient.Stream.CanWrite)
                                for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }

                            myClient.Stream.Write(ByteData, 0, ByteData.Length);
                            myClient.Stream.Flush();

                            bSend = true;

                        }
                        catch { }

                        if(!bSend)
                            for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }
                    }

                    #endregion

                    //Receive Test
                    #region Init_Receive_Test

                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            if (myClient.Instance.Connected && myClient.Instance.Available > 0 && !bReceive)
                            {

                                ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                if (recb == 0) continue; 

                                data = System.Text.Encoding.ASCII.GetString(ByteData);

                                if(data.Length>0)
                                    bReceive = true;
                            }

                        }
                        catch { }

                        if (!bReceive)
                            for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }
                    }

                    #endregion

                    //Send Packet Test
                    #region Init_SendPacket_Test

                    for (int i = 0; i < 2; i++)
                    {

                        dpTest = new DataPacket();
                        dpTest.Message = "Test Data";
                        dpTest.SenderID = AppID;
                        dpTest.SenderType = DataStructures.AppType.Server;
                        dpTest.RecepientID = "Unknown";
                        dpTest.RecepientType = DataStructures.AppType.Client;
                        dpTest.TimeStamp = DateTime.Now;

                        if (bPacketSend) continue;

                        try
                        {
                            data = JsonConvert.SerializeObject(dpTest);
                            ByteData = Encoding.ASCII.GetBytes(data);

                            if (!myClient.Instance.Connected || !myClient.Stream.CanWrite)
                                for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }

                            myClient.Stream.Write(ByteData, 0, ByteData.Length);
                            myClient.Stream.Flush();

                            bPacketSend = true;
                        }
                        catch (Exception spEx)
                        {
                            bPacketSend = false;

                        }

                        if (!bPacketSend)
                            for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }

                    }
                    #endregion

                    //Receive Packet Test
                    #region Init_ReceivePacketTest

                    for (int i = 0; i < 2; i++)
                    {

                        if (bPacketReceive) continue;

                        try
                        {
                            if (myClient.Instance.Connected && myClient.Instance.Available > 0)
                            {

                                ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                if (recb == 0) continue;

                                data = System.Text.Encoding.ASCII.GetString(ByteData);

                                if (!(data.Length > 0))
                                {
                                    bPacketReceive = false;
                                    continue;
                                }

                                dpTest = new DataPacket();
                                dpTest = JsonConvert.DeserializeObject<DataPacket>(data);

                                if (dpTest.SenderID != string.Empty)
                                {
                                    myClient.Name = dpTest.SenderID;
                                    myClient.Type = dpTest.SenderType;
                                    myClient.LastActive = DateTime.Now;

                                    bPacketReceive = true;
                                }
                            }

                        }
                        catch {
                            bPacketReceive = false;
                        }

                        if (!bPacketReceive)
                            for (int j = 0; j <= int.MaxValue; j++) { /* give a small delay */ }
                    }
                    #endregion

                    if(myClient.Name.Length>0)
                    {
                        myClients.Add(myClient);
                    }
                }
            }
            catch(Exception ICex)
            {

            }
        }

        static void Log(string Message)
        {

        }

        static void Log(Exception ex,string Message="")
        {

        }

        static void Log(DataPacket packet)
        {

        }

    }

    
}
