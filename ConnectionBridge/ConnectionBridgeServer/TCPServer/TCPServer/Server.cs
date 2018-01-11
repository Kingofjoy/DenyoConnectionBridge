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

        ConcurrentBag<Client> Clients = new ConcurrentBag<Client>();

        ConcurrentBag<Client> StagingClients = new ConcurrentBag<Client>();

        ConcurrentQueue<DataPacket> ReceivedMessages = new ConcurrentQueue<DataPacket>();


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


        public bool bAcceptLoop = false;

        public bool bReceiveLoop = false; //test

        public bool bMessageProcessorLoop = true;


        #region worker_items

        BackgroundWorker bwServerAcceptance = new BackgroundWorker();
        BackgroundWorker bwReceiver = new BackgroundWorker();
        BackgroundWorker bwConnNegotiator = new BackgroundWorker();

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

                bwServerAcceptance.WorkerSupportsCancellation = true;
                bwServerAcceptance.DoWork += BwServerAcceptance_DoWork;
                //bwServerAcceptance.RunWorkerAsync();

                Log("Accept Hander Initiated");


                bwReceiver.WorkerSupportsCancellation = true;
                bwReceiver.DoWork += BwReceiver_DoWork;
                //bwReceiver.RunWorkerAsync();

                Console.WriteLine("Receive Hander Initiated");

                bwConnNegotiator.WorkerSupportsCancellation = true;
                bwConnNegotiator.DoWork += BwConnNegotiator_DoWork;
                //bwConnNegotiator.RunWorkerAsync();

                Console.WriteLine("Connection Negatiator Initiated");
            }
            catch (Exception ex)
            {
                Log(ex, "Server");
            }
        }

        private void BwConnNegotiator_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Log("Connection Negotiator Running");

                Parallel.ForEach(StagingClients, (myClient) => {
                    try
                    {
                        Log("Connection Negotiator Initiated For Client " + myClient.Name);

                        DateTime dtTimeStageStarted = DateTime.Now;
                        DateTime dtTimeNegEndTime= DateTime.Now + NegotiateTotalTime;
                        int PresentStageId = 0; bool IsPresentStageCompleted = false;
                        int AttemptCount = 0;

                        bool bSend, bReceive, bPacketSend, bPacketReceive;
                        bSend = bReceive = bPacketSend = bPacketReceive = false;

                        byte[] ByteData; string data = ""; DataPacket dpTest = new DataPacket(); List<DataPacket> dpTestPackets = new List<DataPacket>();

                        Log("Connection Negotiator Start " + myClient.Name + " StT: "+ DateTime.Now.ToString("HH:mm:ss:fff")+ " - EdT: "+ dtTimeNegEndTime.ToString("HH:mm:ss:fff"));

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
                                            Log("Server " + myClient.Instance.Connected +" / " + myClient.Stream.CanWrite  + " on Init_Send_Test @ " + AttemptCount);
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
                                                bReceive = true;
                                                Log("Init_Receive_Test Success @ #: "+ AttemptCount + " D: " + data);

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
                                            Log("Server NA / ND on Init_Receive_Test @ "+ AttemptCount);
                                            Log("Server "+ myClient.Instance.Connected  + " / " + myClient.Instance.Available + " on Init_Receive_Test @ "+ AttemptCount);
                                        }
                                    }
                                    catch(Exception ex)
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
                                                //bPacketReceive = true;
                                                Log("Init_ReceivePacket_Test Data Success @ #: " + AttemptCount + " D: " + data);

                                                try
                                                {
                                                    foreach(string oneData in data.Split("|".ToCharArray()))
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
                                                        catch(Exception OneParseEx)
                                                        {
                                                            Log("Init_ReceivePacket_Test OneParseEx #" + OneParseEx.Message);
                                                        }
                                                    }
                                                }
                                                catch(Exception ParseEx)
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
                                            Log("Server "+ myClient.Instance.Connected  + " / " + myClient.Instance.Available + " on Init_Receive_Test @ "+ AttemptCount);
                                        }
                                    }
                                    catch(Exception ex)
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

                        Log("Connection Negotiator Ended For Client " + myClient.Name);

                        if(!string.IsNullOrEmpty(myClient.AuthToken))
                        {
                            Log("Client Initiated Successfully " + myClient.Name);
                        }
                        else
                        {
                            Log("Client Initiation UnSuccessfull " + myClient.Name + " [ " + bSend+bReceive+bPacketSend+bPacketReceive + " ] ");
                        }

                    }
                    catch(Exception iEx)
                    {
                        Log("Negotiator Instance Ex I: " + myClient.Name + " M: " + iEx.Message);
                    }
                });

                Log("Connection Negatiator Completed");
            }
            catch(Exception MEx)
            {
                Log("Connection Negatiator Error Exit. " + MEx.Message);
            }
        }

        private void BwReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BwServerAcceptance_DoWork(object sender, DoWorkEventArgs e)
        {
            AcceptIncomingConnetion();
        }

        public bool Start()
        {
            try
            {
                serverSocket = new TcpListener(IPAddress.Any, ListenerPort);
                serverSocket.Start();

                IsActive = true;

                bAcceptLoop = true;

                bwServerAcceptance.RunWorkerAsync();
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

                        myClient.Name = "S" + StagingClients.Count.ToString("d3") + "_" + rnd.Next(99).ToString("d2");
                        myClient.LastActive = DateTime.Now;

                        StagingClients.Add(myClient);
                        Log("New Client " + myClient.Name + " Added from " + myClient.Instance.Client.LocalEndPoint + " is in staging area.");

                        if (!bwConnNegotiator.IsBusy)
                        {
                            bwConnNegotiator.RunWorkerAsync();
                            Log("Negotiator Started");
                        }
                        else
                            Log("Negotiator already running");

                        continue;

                        //Send Test
                        #region Init_Send_Test

                        ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");



                        //Attempt 1
                        try
                        {
                            if (!myClient.Instance.Connected || !myClient.Stream.CanWrite)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                            myClient.Stream.Write(ByteData, 0, ByteData.Length);
                            myClient.Stream.Flush();
                            Log("Init_Send_Test Success #1");
                            bSend = true;

                        }
                        catch (Exception ex)
                        {
                            bSend = false;
                            Log(ex, "Init_Send_Test");
                        }
                            
                        //Attempt 2
                        if (!bSend)
                        {
                            for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                            try
                            {

                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                myClient.Stream.Flush();
                                Log("Init_Send_Test Success #2");
                                bSend = true;
                            }
                            catch(Exception ex)
                            {
                                bSend = false;
                                Log(ex, "Init_Send_Test");
                            }
                        }
                        

                        #endregion

                        //Receive Test
                        #region Init_Receive_Test

                        
                        try
                        {
                            if (myClient.Instance.Connected && myClient.Instance.Available > 0)
                            {

                                ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                if (recb == 0) continue;

                                data = System.Text.Encoding.ASCII.GetString(ByteData);

                                if (data.Length > 0)
                                {
                                    bReceive = true;
                                    Log("Init_Receive_Test Success #1 : " + data);
                                }
                            }
                            else
                            {
                                Log("ND on Init_Receive_Test 1");
                                bReceive = false;
                            }

                        }
                        catch (Exception ex)
                        {
                            Log(ex, "Init_Receive_Test 1");
                            bReceive = false;
                        }

                        //Attempt 2
                        if (!bReceive)
                        {
                            for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                            try
                            {
                                if (myClient.Instance.Connected && myClient.Instance.Available > 0)
                                {

                                    ByteData = new byte[myClient.Instance.ReceiveBufferSize];
                                    int recb = myClient.Stream.Read(ByteData, 0, myClient.Instance.ReceiveBufferSize);

                                    if (recb == 0) continue;

                                    data = System.Text.Encoding.ASCII.GetString(ByteData);

                                    if (data.Length > 0)
                                    {
                                        bReceive = true;
                                        Log("Init_Receive_Test Success #2");
                                    }
                                }
                                else
                                {
                                    Log("ND on Init_Receive_Test 2");
                                }

                            }
                            catch (Exception ex)
                            {
                                Log(ex, "Init_Receive_Test 1");
                            }
                        }
                        

                        #endregion

                        //Send Packet Test
                        #region Init_SendPacket_Test

                        
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
                            ByteData = Encoding.ASCII.GetBytes(data);

                            if (!myClient.Instance.Connected || !myClient.Stream.CanWrite)
                                for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                            myClient.Stream.Write(ByteData, 0, ByteData.Length);
                            myClient.Stream.Flush();
                            Log("Init_SendPacket_Test Success #1");
                            bPacketSend = true;
                        }
                        catch (Exception spEx)
                        {
                            Log(spEx, "Init_SendPacket_Test 1");
                            bPacketSend = false;
                        }

                        if (!bPacketSend)
                        {
                            for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

                            try
                            {
                                data = JsonConvert.SerializeObject(dpTest);
                                ByteData = Encoding.ASCII.GetBytes(data);


                                myClient.Stream.Write(ByteData, 0, ByteData.Length);
                                myClient.Stream.Flush();
                                Log("Init_SendPacket_Test Success #2");
                                bPacketSend = true;
                            }
                            catch (Exception spEx)
                            {
                                Log(spEx, "Init_SendPacket_Test 2");
                                bPacketSend = false;
                            }
                        }

                        #endregion

                        //Receive Packet Test
                        #region Init_ReceivePacketTest


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
                                    Log("Init_ReceivePacketTest Success #1");
                                }
                            }
                            else
                                Log("ND on Init_ReceivePacketTest 1");

                        }
                        catch(Exception ex)
                        {
                            Log(ex, "Init_ReceivePacketTest 1");
                            bPacketReceive = false;
                        }

                        //Attempt 2
                        if (!bPacketReceive)
                        {
                            for (int j = 0; j <= 990000000; j++) { /* give a small delay */ }

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
                                        Log("Init_ReceivePacketTest Success #2");
                                        bPacketReceive = true;
                                    }
                                }
                                else
                                    Log("ND on Init_ReceivePacketTest 2");

                            }
                            catch (Exception ex)
                            {
                                Log(ex, "Init_ReceivePacketTest 1");
                                bPacketReceive = false;
                            }
                        }
                        
                        #endregion

                        if (!string.IsNullOrEmpty(myClient.Name) && myClient.Name.Length > 0)
                        {
                            Clients.Add(myClient);
                            bReceiveLoop = true;
                            Log("New Client " + myClient.Name + " Added from " + myClient.Instance.Client.LocalEndPoint);
                        }
                        else
                        {
                            try {
                                Log("Unable to accept client request. [" + myClient.Name + " , " + myClient.Instance.Client.LocalEndPoint + ", Send: " + bSend + ", Receive: " + bReceive + ", PacketSend: " + bPacketSend + ", PacketReceived: " + bPacketReceive+ "]");
                            }
                            catch
                            {
                                Log("Unable to accept client request. [" + myClient.Name + " ,  , Send: " + bSend + ", Receive: " + bReceive + ", PacketSend: " + bPacketSend + ", PacketReceived: " + bPacketReceive + "]");
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
