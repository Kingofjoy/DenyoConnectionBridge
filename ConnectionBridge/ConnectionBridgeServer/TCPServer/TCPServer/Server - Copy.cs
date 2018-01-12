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

    public class Server
    {
        TcpListener serverSocket = null;

        ConcurrentBag<Client> Clients = new ConcurrentBag<Client>();

        ConcurrentQueue<DataPacket> ReceivedMessages = new ConcurrentQueue<DataPacket>();

        DateTime dtNext;
        bool bHBInit = false;

        public bool bAcceptLoop = false;

        public bool bReceiveLoop = true; //test

        public bool bMessageProcessorLoop = true;

        public int ListenerPort { get { return _ListenerPort; } }
        int _ListenerPort;

        public bool IsHeartBeatActive = false;
        int HeartBeatInterval;

        public bool IsActive { get; set; }

        public string AppID { get; set; }

        public string AppType { get; set; }

        public string AuthToken { get; set; }


        public Server()
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
            catch (Exception ex)
            {
                Log(ex, "Server");
            }
        }

        public bool Start()
        {
            try
            {
                serverSocket = new TcpListener(IPAddress.Any, ListenerPort);
                serverSocket.Start();

                IsActive = true;

                bAcceptLoop = true;
            }
            catch (Exception ex)
            {
                Log(ex, "Start");
            }
            return IsActive;
        }

        public void AcceptIncomingConnetion()
        {
            Byte[] ByteData;

            try
            {
                while (bAcceptLoop)
                {

                    //Log("accept loop A");
                    //for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }
                    //Log("accept loop B");

                    bool bSend, bReceive, bPacketSend, bPacketReceive;
                    bSend = bReceive = bPacketSend = bPacketReceive = false;
                    string data = string.Empty;
                    DataPacket dpTest = new DataPacket();

                    while (serverSocket.Pending())
                    {
                        Log("Incoming new client request");
                        Client myClient = new Client();
                        myClient.Instance = serverSocket.AcceptTcpClient();
                        myClient.Stream = myClient.Instance.GetStream();

                        //Send Test
                        #region Init_Send_Test

                        ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                        for (int i = 0; i < 2; i++)
                        {
                            if (bSend) continue;

                            try
                            {
                                if (!myClient.Instance.Connected || !myClient.Stream.CanWrite)
                                    for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                myClient.Stream.Flush();

                                bSend = true;

                            }
                            catch { }

                            if (!bSend)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }
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

                                    if (data.Length > 0)
                                        bReceive = true;
                                }

                            }
                            catch { }

                            if (!bReceive)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }
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
                                    for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                myClient.Stream.Flush();

                                bPacketSend = true;
                            }
                            catch (Exception spEx)
                            {
                                bPacketSend = false;

                            }

                            if (!bPacketSend)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

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
                            catch
                            {
                                bPacketReceive = false;
                            }

                            if (!bPacketReceive)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }
                        }
                        #endregion

                        if (string.IsNullOrEmpty(myClient.Name) && myClient.Name.Length > 0)
                        {
                            Clients.Add(myClient);
                            bReceiveLoop = true;
                            Log("New Client " + myClient.Name + " Added from " + myClient.Instance.Client.LocalEndPoint);
                        }
                        else
                        {
                            try {
                                Log("Unable to accept client request. [" + myClient.Name + " , " + myClient.Instance.Client.LocalEndPoint + ", Send: " + bSend + ", Receive: " + bReceive + ", PacketSend: " + bPacketSend + ", bPacketSend: " + bPacketSend + "]");
                            }
                            catch
                            {
                                Log("Unable to accept client request. [" + myClient.Name + " ,  , Send: " + bSend + ", Receive: " + bReceive + ", PacketSend: " + bPacketSend + ", bPacketSend: " + bPacketSend + "]");
                            }
                        }
                    }

                    //Log("ACCEPT loop C");
                }
            }
            catch (Exception ICex)
            {

            }
        }

        public void ReceiveMessages()
        {
            try
            {
                while(bReceiveLoop)
                {

                    Log("RECEIVE loop A");
                    for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }
                    Log("RECEIVE loop B");

                    foreach (Client cliobj in Clients)
                    {
                        while (cliobj.Instance.Connected && cliobj.Instance.Available > 0)
                        {

                            try
                            {
                                byte[] bytesFrom = new byte[cliobj.Instance.ReceiveBufferSize];

                                int recb = cliobj.Stream.Read(bytesFrom, 0, (int)cliobj.Instance.ReceiveBufferSize);

                                if (recb == 0) continue;

                                string data = System.Text.Encoding.ASCII.GetString(bytesFrom);

                                if (data[0] == '\0') continue ;

                                DataPacket dpReceived = new DataPacket();
                                dpReceived  = JsonConvert.DeserializeObject<DataPacket>(data);

                                ReceivedMessages.Enqueue(dpReceived);
                            }
                            catch (Exception ex2)
                            {
                                Log(ex2, "ReceiveMsg:Er:[" + cliobj.Name + "]");
                                //todo: check if client still connected
                            }

                                

                        }
                    }

                    Log("RECEIVE loop C");
                }

            }
            catch(Exception ex)
            {
                Log(ex, "ReceiveMessages");
            }

        }



        public bool SendMessage(DataPacket dpToSend)
        {
            bool iSMsgSent = false;

            try
            {
                Client TargetClient = Clients.FirstOrDefault(x => (x.Name == dpToSend.RecepientID));
                if(TargetClient.Instance.Connected)
                {
                    if ((TargetClient.Instance.Available > 0))
                        for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                    Byte[] sendBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dpToSend));
                    TargetClient.Stream.Write(sendBytes, 0, sendBytes.Length);
                    TargetClient.Stream.Flush();
                }
            }
            catch(Exception ex)
            {
                Log(ex, "Error on SendMessage");
            }

            return iSMsgSent;
        }
        void Log(string Message)
        {
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmss") + " : " + Message);
        }

        void Log(Exception ex, string Message = "")
        {
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmss") + " : Error : " + Message + " > " + ex.Message);
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmss") + " : Error Trace : " + ex.StackTrace);
        }

        void Log(DataPacket packet)
        {

        }

    }


}
