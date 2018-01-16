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
using System.ComponentModel;

namespace Denyo.ConnectionBridge.Server.TCPServer
{

    public class Server
    {
        TcpListener serverSocket = null;

        ConcurrentDictionary<string,Client> Clients = new ConcurrentDictionary<string, Client>();

        ConcurrentQueue<Client> StagingClients = new ConcurrentQueue<Client>();

        ConcurrentQueue<DataPacket> ReceivedMessages = new ConcurrentQueue<DataPacket>();
        ConcurrentQueue<DataPacket> PostMessages = new ConcurrentQueue<DataPacket>();


        DateTime dtNext;
        TimeSpan NegotiateStageTime, NegotiateTotalTime;
        bool bHBInit = false;
        Random rnd = new Random();


        public int ListenerPort { get { return _ListenerPort; } }
        int _ListenerPort;

        public bool IsHeartBeatActive = false;
        int HeartBeatInterval;

        public bool IsActive { get; set; }

        public string AppID { get; set; }

        public string AppType { get; set; }

        public string AuthToken { get; set; }


        bool bAcceptLoop = false;
        bool bNegotiationLoop = false;
        bool bReceiveLoop = false;
        bool bMessageProcessorLoop = false;
        bool bMasterServerProcess= true;



        #region worker_items

        BackgroundWorker bwServerAcceptance = new BackgroundWorker();
        BackgroundWorker bwReceiver = new BackgroundWorker();
        BackgroundWorker bwConnNegotiator = new BackgroundWorker();

        BackgroundWorker bwProcessor = new BackgroundWorker();

        #endregion


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

                NegotiateStageTime = new TimeSpan(0, 0, 0, 1, 200);
                NegotiateTotalTime = new TimeSpan(0, 0, 10, 1);

            }
            catch (Exception ex)
            {
                Log(ex, "Server Initialization Error");
            }
        }

        private void BwProcessor_DoWork(object sender, DoWorkEventArgs e)
        {
            while (bMasterServerProcess)
            {
                try
                {
                    if(this.IsActive)
                    {
                        if(!bAcceptLoop)
                            new Task(AcceptIncoming_Connection).Start();

                        if(!bNegotiationLoop && StagingClients.Count>0)
                            new Task(Negotiate_Connection).Start();

                        if(!bReceiveLoop)
                            new Task(ReceiveMessages).Start();

                    }
                }
                catch (Exception mwp_ex)
                {
                    Log(mwp_ex, "TCP Processor Error");
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        private void BwConnNegotiator_DoWork(object sender, DoWorkEventArgs e)
        {
            Negotiate_Connection();
        }

        private void BwServerAcceptance_DoWork(object sender, DoWorkEventArgs e)
        {
            AcceptIncoming_Connection();
        }

        private void BwReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            ReceiveMessages();
        }



        public bool Start()
        {
            try
            {
                serverSocket = new TcpListener(IPAddress.Any, ListenerPort);
                serverSocket.Start();

                IsActive = true;

                bwProcessor.WorkerSupportsCancellation = true;
                bwProcessor.DoWork += BwProcessor_DoWork;
                bwProcessor.RunWorkerAsync();


                #region  //Using Indidual Background Workers
                /* 
                //Using Indidual Background Workers

                //bwServerAcceptance.WorkerSupportsCancellation = true;
                //bwServerAcceptance.DoWork += BwServerAcceptance_DoWork;
                //bwServerAcceptance.RunWorkerAsync();

                //Log("Accept Hander Initiated");


                //bwReceiver.WorkerSupportsCancellation = true;
                //bwReceiver.DoWork += BwReceiver_DoWork;
                //bwReceiver.RunWorkerAsync();

                //Console.WriteLine("Receive Hander Initiated");

                //bwConnNegotiator.WorkerSupportsCancellation = true;
                //bwConnNegotiator.DoWork += BwConnNegotiator_DoWork;
                //bwConnNegotiator.RunWorkerAsync();

                //Console.WriteLine("Connection Negatiator Initiated");

                */

                #endregion

            }
            catch (Exception ex)
            {
                Log(ex, "Start");
            }
            return IsActive;
        }

        public void AcceptIncoming_Connection()
        {

            try
            {
                bAcceptLoop = true;

                //while (bAcceptLoop)
                {

                    while (serverSocket.Pending())
                    {
                        Log("Incoming new client request");

                        Client myClient = new Client();
                        myClient.Instance = serverSocket.AcceptTcpClient();
                        myClient.Stream = myClient.Instance.GetStream();

                        myClient.Name = "S" + StagingClients.Count.ToString("d3") + "_" + rnd.Next(99).ToString("d3");

                        myClient.LastActive = DateTime.Now;

                        StagingClients.Enqueue(myClient);

                        Log("New Client " + myClient.Name + " Added from " + myClient.Instance.Client.LocalEndPoint + " is in staging area.");

                        //if (!bwConnNegotiator.IsBusy)
                        //{
                        //    bwConnNegotiator.RunWorkerAsync();
                        //    Log("Negotiator Started");
                        //}
                        //else
                        //    Log("Negotiator already running");
                    }

                    //System.Threading.Thread.Sleep(200);
                }

                bAcceptLoop = false;
            }
            catch (Exception ICex)
            {
                Log(ICex,"Accept Incoming Connection Er");
            }
        }

        private void Negotiate_Connection()
        {
            try
            {
                bNegotiationLoop = true;
                Log("Connection Negotiator Running");

                List<string> ErrorConnections = new List<string>();
                Client myClient;
                while (StagingClients.TryDequeue(out myClient))
                { 
                    try
                    {

                        Log("Connection Negotiator Initiated For Client " + myClient.Name);

                        DateTime dtTimeStageStarted = DateTime.Now;
                        DateTime dtTimeNegEndTime = DateTime.Now + NegotiateTotalTime;
                        int PresentStageId = 0; bool IsPresentStageCompleted = false;
                        int AttemptCount = 0;

                        bool bSend, bReceive, bPacketSend, bPacketReceive;
                        bSend = bReceive = bPacketSend = bPacketReceive = false;

                        byte[] ByteData; string data = ""; DataPacket dpTest = new DataPacket(); List<DataPacket> dpTestPackets = new List<DataPacket>();

                        Log("Connection Negotiator Start " + myClient.Name + " StT: " + DateTime.Now.ToString("HH:mm:ss:fff") + " - EdT: " + dtTimeNegEndTime.ToString("HH:mm:ss:fff"));

                        try
                        {
                            while ((DateTime.Now <= dtTimeNegEndTime) && string.IsNullOrEmpty(myClient.AuthToken))
                            {
                                switch (PresentStageId)
                                {
                                    case 0: // Write Test (New)

                                        #region Init_Send_Test

                                        Log("Init_Send_Test Started #" + AttemptCount);

                                        try
                                        {
                                            ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                                            if (myClient.Instance.Connected && myClient.Stream.CanWrite)
                                            {

                                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                                myClient.Stream.Flush();

                                                Log("Init_Send_Test Success @ #" + AttemptCount);
                                                bSend = true;
                                                PresentStageId++; AttemptCount = 0;
                                            }
                                            else
                                            {
                                                Log("Server NA / Non-Writable on Init_Send_Test @ " + AttemptCount);
                                                Log("Server " + myClient.Instance.Connected + " / " + myClient.Stream.CanWrite + " on Init_Send_Test @ " + AttemptCount);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Log(ex, "Init_Send_Test #" + AttemptCount);
                                        }

                                        if (!bSend)
                                        {
                                            Log("Init_Send_Test Failed @ #" + AttemptCount);
                                            for (int j = 0; j <= 4000000; j++) { /* give a small delay */ }
                                            AttemptCount++;
                                        }

                                        #endregion
                                        break;

                                    case 1: // ReceiveTest (0-Write-Completed)

                                        #region Init_Receive_Test

                                        Log("Init_Receive_Test Started #" + AttemptCount);

                                        try
                                        {
                                            if (myClient.Instance.Connected && myClient.Instance.Available > 0)
                                            {

                                                ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                                int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                                if (recb == 0)
                                                {
                                                    Log("Init_Receive_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                                    continue;
                                                }
                                                else
                                                    Log("Init_Receive_Test DataCount 0 Length @ #: " + AttemptCount);

                                                data = System.Text.Encoding.ASCII.GetString(ByteData);

                                                if (data.Length > 0)
                                                {
                                                    data = data.Replace("\0", "");
                                                    bReceive = true;
                                                    Log("Init_Receive_Test Success @ #: " + AttemptCount + " D: " + data);

                                                    PresentStageId++;
                                                    AttemptCount = 0;
                                                }
                                                else
                                                {
                                                    Log("Init_Receive_Test Data 0 Length @ #: " + AttemptCount);
                                                }
                                            }
                                            else
                                            {
                                                Log("Server NA / ND on Init_Receive_Test @ " + AttemptCount);
                                                Log("Server " + myClient.Instance.Connected + " / " + myClient.Instance.Available + " on Init_Receive_Test @ " + AttemptCount);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log(ex, "Init_Receive_Test #" + AttemptCount);
                                        }

                                        if (!bReceive)
                                        {
                                            Log("Init_Receive_Test Failed @ #" + AttemptCount);
                                            for (int j = 0; j <= 1000000; j++) { /* give a small delay */ }
                                            AttemptCount++;
                                        }


                                        #endregion Init_Receive_Test
                                        break;

                                    case 2: // SendPacket (0-Write-Completed, 1-Receive-Completed)

                                        #region Init_SendPacket_Test
                                        Log("Init_SendPacket_Test Started @ #" + AttemptCount);

                                        dpTest = new DataPacket();
                                        dpTest.Message = "Server Test Data";
                                        dpTest.SenderID = AppID;
                                        dpTest.SenderType = DataStructures.AppType.Server;
                                        dpTest.RecepientID = "Unknown";
                                        dpTest.RecepientType = DataStructures.AppType.Client;
                                        dpTest.TimeStamp = DateTime.Now;

                                        try
                                        {
                                            data = JsonConvert.SerializeObject(dpTest);
                                            ByteData = Encoding.ASCII.GetBytes(data + "|");

                                            if (myClient.Instance.Connected && myClient.Stream.CanWrite)
                                            {

                                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                                myClient.Stream.Flush();

                                                Log("Init_SendPacket_Test Success @ #" + AttemptCount);
                                                bPacketSend = true;
                                                PresentStageId++; AttemptCount = 0;
                                            }
                                            else
                                            {
                                                Log("Server NA / Non-Writable on Init_SendPacket_Test @ " + AttemptCount);
                                                Log("Server " + myClient.Instance.Connected + " / " + myClient.Stream.CanWrite + " on Init_SendPacket_Test @ " + AttemptCount);
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Log(ex, "Init_SendPacket_Test #" + AttemptCount);
                                        }


                                        if (!bPacketSend)
                                        {
                                            Log("Init_SendPacket_Test Failed @ #" + AttemptCount);
                                            for (int j = 0; j <= 4000000; j++) { /* give a small delay */ }
                                            AttemptCount++;
                                        }


                                        #endregion Init_SendPacket_Test
                                        break;

                                    case 3: // ReceivePacket (0-Write-Completed, 1-Receive-Completed, 2-SendPacketCompleted)

                                        #region Init_ReceivePacket_Test

                                        Log("Init_ReceivePacket_Test Started #" + AttemptCount);

                                        try
                                        {
                                            if (myClient.Instance.Connected && myClient.Instance.Available > 0)
                                            {

                                                ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                                int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                                if (recb == 0)
                                                {
                                                    Log("Init_ReceivePacket_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                                    continue;
                                                }
                                                else
                                                    Log("Init_ReceivePacket_Test DataCount 0 Length @ #: " + AttemptCount);

                                                data = System.Text.Encoding.ASCII.GetString(ByteData);

                                                if (data.Length > 0)
                                                {
                                                    data = data.Replace("\0", "");
                                                    //bPacketReceive = true;
                                                    Log("Init_ReceivePacket_Test Data Success @ #: " + AttemptCount + " D: " + data);

                                                    try
                                                    {
                                                        foreach (string oneData in data.Split("|".ToCharArray()))
                                                        {
                                                            try
                                                            {
                                                                if (bPacketReceive) continue;

                                                                dpTest = new DataPacket();
                                                                dpTest = JsonConvert.DeserializeObject<DataPacket>(oneData);

                                                                if (!string.IsNullOrEmpty(dpTest.SenderID))
                                                                {
                                                                    myClient.Name = dpTest.SenderID;
                                                                    myClient.Type = dpTest.SenderType;
                                                                    myClient.LastActive = DateTime.Now;

                                                                    bPacketReceive = true;
                                                                    Log("Init_ReceivePacket_Test Success #" + AttemptCount);

                                                                    PresentStageId++;
                                                                    AttemptCount = 0;

                                                                }
                                                                else
                                                                    Log("Init_ReceivePacket_Test OneParse Err SenderID Empty");
                                                            }
                                                            catch (Exception OneParseEx)
                                                            {
                                                                Log("Init_ReceivePacket_Test OneParseEx #" + OneParseEx.Message);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ParseEx)
                                                    {
                                                        Log("Init_ReceivePacket_Test ParseEx #" + ParseEx.Message);
                                                    }

                                                }
                                                else
                                                {
                                                    Log("Init_ReceivePacket_Test Data 0 Length @ #: " + AttemptCount);
                                                }
                                            }
                                            else
                                            {
                                                Log("Server NA / ND on Init_ReceivePacket_Test @ " + AttemptCount);
                                                Log("Server " + myClient.Instance.Connected + " / " + myClient.Instance.Available + " on Init_Receive_Test @ " + AttemptCount);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log(ex, "Init_ReceivePacket_Test #" + AttemptCount);
                                        }

                                        if (!bPacketReceive)
                                        {
                                            Log("Init_ReceivePacket_Test Failed @ #" + AttemptCount);
                                            for (int j = 0; j <= 1000000; j++) { /* give a small delay */ }
                                            AttemptCount++;
                                        }


                                        #endregion Init_ReceivePacket_Test
                                        break;

                                    default:
                                        Log("Unknown Stage ID" + PresentStageId);
                                        break;
                                }

                                //(0 - Write - Completed, 1 - Receive - Completed, 2 - SendPacketCompleted, 3 - ReceivePacket)
                                if (bSend && bReceive && bPacketReceive && bPacketSend)
                                    myClient.AuthToken = Clients.Count.ToString("d3");
                                else
                                    System.Threading.Thread.Sleep(100);

                            }
                        }
                        catch (Exception ex)
                        {
                            Log(ex, "Client Initiation Error");
                            Log("Client Initiation Error. " + myClient.Name + ". ");
                        }

                        Log("Connection Negotiator Ended For Client " + myClient.Name);

                        if (!string.IsNullOrEmpty(myClient.AuthToken))
                        {
                            Clients.AddOrUpdate(myClient.Name, myClient, (nm,ev) => {return myClient;});
                            Log("Client Initiated Successfully " + myClient.Name);
                        }
                        else
                        {
                            Log("Client Initiation UnSuccessfull " + myClient.Name + " [ " + bSend + bReceive + bPacketSend + bPacketReceive + " ] ");
                        }

                    }
                    catch (Exception iEx)
                    {
                        Log("Negotiator Instance Ex I: " + myClient.Name + " M: " + iEx.Message);
                    }
                }

                Log("Connection Negatiator Completed");

                bNegotiationLoop = false;
            }
            catch (Exception MEx)
            {
                Log("Connection Negatiator Error Exit. " + MEx.Message);
                bNegotiationLoop = false;
            }

        }

        public void ReceiveMessages()
        {
            try
            {
                bReceiveLoop = true;
                //while(bReceiveLoop)
                {

                    foreach (Client cliobj in Clients.Values)
                    {
                        
                        try
                        {
                            if (cliobj.Instance.Connected && cliobj.Instance.Available > 0)
                            {

                                byte[] bytesFrom = new byte[cliobj.Instance.ReceiveBufferSize];

                                int recb = cliobj.Stream.Read(bytesFrom, 0, (int)cliobj.Instance.ReceiveBufferSize);

                                if (recb == 0) continue;

                                string data = System.Text.Encoding.ASCII.GetString(bytesFrom);

                                if (data.Length <= 0) continue;
                                if (data[0] == '\0') continue;

                                data = data.Replace("\0", "");

                                foreach (string oneData in data.Split("|".ToCharArray()))
                                {
                                    if (string.IsNullOrEmpty(oneData)) continue;
                                    try
                                    {
                                        DataPacket dpReceived = new DataPacket();
                                        dpReceived = JsonConvert.DeserializeObject<DataPacket>(oneData.Replace("|",""));
                                        ReceivedMessages.Enqueue(dpReceived);
                                        Log("Client: " + cliobj.Name + " > Data: " + oneData);
                                    }
                                    catch (Exception OnePex)
                                    {
                                        Log(OnePex, "One-Parse-Exception");
                                        Log("Client: " + cliobj.Name + " ; Data: " + oneData);
                                    }
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            Log(ex2, "ReceiveMsg:Er:[" + cliobj.Name + "]");
                            //todo: check if client still connected
                        }

                    }

                }

                bReceiveLoop = false;

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
                Client TargetClient;
                Clients.TryGetValue(dpToSend.RecepientID, out TargetClient);
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
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmssfff") + " : " + Message);
        }

        void Log(Exception ex, string Message = "")
        {
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmssfff") + " : Error : " + Message + " > " + ex.Message);
            Console.WriteLine(DateTime.Now.ToString("ddMMyyyy HHmmssfff") + " : Error Trace : " + ex.StackTrace);
        }

        void Log(DataPacket packet)
        {

        }

    }


}
