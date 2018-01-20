using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.DataStructures;
using Newtonsoft.Json;

namespace Denyo.ConnectionBridge.Client
{
    public class TcpClientHandler
    {
        TcpClient Client { get; set; }

        NetworkStream ServerStream { get; set; }

        string CmdFromServer = string.Empty;

        BackgroundWorker bwReceiver = new BackgroundWorker();

        public Main FormRef { get; set; }

        private string AuthToken { get; set; }

        private AppType Type { get; set; }

        private string AppID { get; set; }


        private string RSName { get; set; }

        private int RSPort { get; set; }

        DataPacket cmd = new DataPacket();

        public string ServerID = string.Empty;

        public bool IsServerConnected { get; set; }

        public TcpClientHandler()
        {
            try
            {
                AppID = ConfigurationManager.AppSettings["AppID"];
                Type = (AppType) Enum.Parse(typeof(AppType), ConfigurationManager.AppSettings["AppType"].ToString());
                AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                RSName = ConfigurationManager.AppSettings["RServer"];
                RSPort = int.Parse(ConfigurationManager.AppSettings["RSPort"]);

                Client = new TcpClient();
                IsServerConnected = InitiateConnection();

                //Client.Connect(RSName, RSPort);

                //bwReceiver.WorkerSupportsCancellation = true;
                //bwReceiver.DoWork += BwReceiver_DoWork;
                //bwReceiver.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.Log("TCP Handler Initialization failed.", ex);
            }
        }

        bool InitiateConnection()
        {
            bool bSend, bReceive, bPacketSend, bPacketReceive;
            bSend = bReceive = bPacketSend = bPacketReceive = false;
            string data = "";
            byte[] ByteData;

            DataPacket dpTest = new DataPacket();
            try
            {
                Logger.Log("Connecting to server...");

                Client.Connect(RSName, RSPort);

                ServerStream = Client.GetStream();

                Logger.Log("Connection Negotiator Initiated For Server ");

                AuthToken = "";
                DateTime dtTimeStageStarted = DateTime.Now;
                TimeSpan NegotiateStageTime, NegotiateTotalTime;

                NegotiateStageTime = new TimeSpan(0, 0, 0, 1, 200);
                NegotiateTotalTime = new TimeSpan(0, 0, 1, 0);

                DateTime dtTimeNegEndTime = DateTime.Now + NegotiateTotalTime;
                int PresentStageId = 0; 
                int AttemptCount = 0;

                //bool bSend, bReceive, bPacketSend, bPacketReceive;
                bSend = bReceive = bPacketSend = bPacketReceive = false;

                List<DataPacket> dpTestPackets = new List<DataPacket>();

                Logger.Log("Connection Negotiator Server Start " + " StT: " + DateTime.Now.ToString("HH:mm:ss:fff") + " - EdT: " + dtTimeNegEndTime.ToString("HH:mm:ss:fff"));

                while ((DateTime.Now <= dtTimeNegEndTime) && string.IsNullOrEmpty(AuthToken))
                {
                    switch (PresentStageId)
                    {
                        case 0: // ReceiveTest (New)

                            #region Init_Receive_Test

                            Logger.Log("Init_Receive_Test Started #" + AttemptCount);

                            try
                            {
                                Logger.Log("Server " + Client.Connected + " / " + ServerStream.DataAvailable + " / " + Client.Available + " on Init_Receive_Test @ " + AttemptCount);

                                if (Client.Connected && ServerStream.DataAvailable)
                                {

                                    ByteData = new byte[Client.ReceiveBufferSize];
                                    int recb = ServerStream.Read(ByteData, 0, Client.ReceiveBufferSize);

                                    if (recb == 0)
                                    {
                                        Logger.Log("Init_Receive_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                        continue;
                                    }
                                    else
                                        Logger.Log("Init_Receive_Test DataCount " + recb + " Length @ #: " + AttemptCount);

                                    data = System.Text.Encoding.ASCII.GetString(ByteData);

                                    if (data.Length > 0)
                                    {
                                        bReceive = true;
                                        Logger.Log("Init_Receive_Test Success @ #: " + AttemptCount + " D: " + data);

                                        PresentStageId++;
                                        AttemptCount = 0;
                                    }
                                    else
                                    {
                                        Logger.Log("Init_Receive_Test Data 0 Length @ #: " + AttemptCount);
                                    }
                                }
                                else
                                {
                                    Logger.Log("Server NA / ND on Init_Receive_Test @ " + AttemptCount);
                                    Logger.Log("Server " + Client.Connected + " / " + ServerStream.DataAvailable + " on Init_Receive_Test @ " + AttemptCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Init_Receive_Test #" + AttemptCount + " Err: " + ex.Message);
                            }

                            if (!bReceive)
                            {
                                Logger.Log("Init_Receive_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 1000000; j++) { } // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_Receive_Test
                            break;

                        case 1: // Write Test (0-ReceiveTest)

                            #region Init_Send_Test

                            Logger.Log("Init_Send_Test Started #" + AttemptCount);

                            try
                            {
                                ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                                if (Client.Connected && ServerStream.CanWrite)
                                {

                                    ServerStream.Write(ByteData, 0, ByteData.Length);
                                    ServerStream.Flush();

                                    Logger.Log("Init_Send_Test Success @ #" + AttemptCount);
                                    bSend = true;
                                    PresentStageId++; AttemptCount = 0;
                                }
                                else
                                {
                                    Logger.Log("Server NA / Non-Writable on Init_Send_Test @ " + AttemptCount);
                                    Logger.Log("Server " + Client.Connected + " / " + ServerStream.CanWrite + " on Init_Send_Test @ " + AttemptCount);
                                }


                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Init_Send_Test Error on #" + AttemptCount + " Error: " + ex.Message);
                            }

                            if (!bSend)
                            {
                                Logger.Log("Init_Send_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 4000000; j++) { } // give a small delay
                                AttemptCount++;
                            }

                            #endregion
                            break;

                        case 2: // ReceivePacket (0-Write-Completed, 1-Receive-Completed, 2-SendPacketCompleted)

                            #region Init_ReceivePacket_Test

                            Logger.Log("Init_ReceivePacket_Test Started #" + AttemptCount);

                            try
                            {

                                Logger.Log("Server " + Client.Connected + " / " + Client.Available + " / " + ServerStream.DataAvailable + "  on Init_ReceivePacket_Test @ " + AttemptCount);

                                if (Client.Connected && Client.Available > 0)
                                {

                                    ByteData = new byte[Client.ReceiveBufferSize];
                                    int recb = ServerStream.Read(ByteData, 0, Client.ReceiveBufferSize);
                                    if (recb == 0)
                                    {
                                        Logger.Log("Init_ReceivePacket_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                        continue;
                                    }
                                    else
                                        Logger.Log("Init_ReceivePacket_Test DataCount " + recb + " Length @ #: " + AttemptCount);

                                    data = System.Text.Encoding.ASCII.GetString(ByteData);

                                    if (data.Length > 0)
                                    {
                                        //bPacketReceive = true;
                                        Logger.Log("Init_ReceivePacket_Test Success @ #: " + AttemptCount + " D: " + data);

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
                                                        ServerID = dpTest.SenderID;

                                                        bPacketReceive = true;
                                                        Logger.Log("Init_ReceivePacket_Test Success #" + AttemptCount);

                                                        PresentStageId++;
                                                        AttemptCount = 0;

                                                    }
                                                    else
                                                        Logger.Log("Init_ReceivePacket_Test OneParse Err SenderID Empty");
                                                }
                                                catch (Exception OneParseEx)
                                                {
                                                    Logger.Log("Init_ReceivePacket_Test OneParseEx #" + OneParseEx.Message);
                                                }
                                            }
                                        }
                                        catch (Exception ParseEx)
                                        {
                                            Logger.Log("Init_ReceivePacket_Test ParseEx #" + ParseEx.Message);
                                        }

                                    }
                                    else
                                    {
                                        Logger.Log("Init_ReceivePacket_Test Data 0 Length @ #: " + AttemptCount);
                                    }
                                }
                                else
                                {
                                    Logger.Log("Server NA / ND on Init_ReceivePacket_Test @ " + AttemptCount);
                                    Logger.Log("Server " + Client.Connected + " / " + Client.Available + " on Init_ReceivePacket_Test @ " + AttemptCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Init_ReceivePacket_Test #" + AttemptCount + " Err" + ex.Message);
                            }

                            if (!bPacketReceive)
                            {
                                Logger.Log("Init_ReceivePacket_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 1000000; j++) { } // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_ReceivePacket_Test
                            break;

                        case 3: // SendPacket (0-Write-Completed, 1-Receive-Completed)

                            #region Init_SendPacket_Test
                            Logger.Log("Init_SendPacket_Test Started @ #" + AttemptCount);

                            dpTest = new DataPacket();
                            dpTest.Message = "Client Test Data";
                            dpTest.SenderID = AppID;
                            dpTest.SenderType = AppType.Server;
                            dpTest.RecepientID = "Unknown";
                            dpTest.RecepientType = AppType.Client;
                            dpTest.TimeStamp = DateTime.Now;

                            try
                            {
                                data = JsonConvert.SerializeObject(dpTest);
                                ByteData = Encoding.ASCII.GetBytes(data + "|");

                                if (Client.Connected && ServerStream.CanWrite)
                                {

                                    ServerStream.Write(ByteData, 0, ByteData.Length);
                                    ServerStream.Flush();

                                    Logger.Log("Init_SendPacket_Test Success @ #" + AttemptCount);
                                    bPacketSend = true;
                                    PresentStageId++; AttemptCount = 0;
                                }
                                else
                                {
                                    Logger.Log("Server NA / Non-Writable on Init_SendPacket_Test @ " + AttemptCount);
                                    Logger.Log("Server " + Client.Connected + " / " + ServerStream.CanWrite + " on Init_SendPacket_Test @ " + AttemptCount);
                                }


                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Init_SendPacket_Test #" + AttemptCount + " Err: " + ex.Message);
                            }


                            if (!bPacketSend)
                            {
                                Logger.Log("Init_SendPacket_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 4000000; j++) { } // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_SendPacket_Test
                            break;

                        default:
                            Logger.Log("Unknown Stage ID" + PresentStageId);
                            break;
                    }

                    //(0 - Write - Completed, 1 - Receive - Completed, 2 - SendPacketCompleted, 3 - ReceivePacket)
                    if (bSend && bReceive && bPacketReceive && bPacketSend)
                        AuthToken = ServerID;
                    else
                        System.Threading.Thread.Sleep(100);
                }

                Logger.Log("Connection Negotiator Ended For Server");

                if (!string.IsNullOrEmpty(AuthToken))
                {
                    Logger.Log("Client Sucessfully registered " + ServerID);
                    return true;
                }
                else
                {
                    Logger.Log("Client Initiation UnSuccessfull " + ServerID + " [ " + bSend + bReceive + bPacketSend + bPacketReceive + " ] ");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logger.Log("Error while connecting to server");
                Logger.Log("Client Initiation UnSuccessfull " + ServerID + " [ " + bSend + bReceive + bPacketSend + bPacketReceive + " ] ");
                Logger.Log(ex.Message);
                return false;
            }
        }


        private void BwReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (Client.Connected)
                {
                    try
                    {
                        if (!ServerStream.DataAvailable) continue;

                        byte[] inStream = new byte[Client.ReceiveBufferSize];

                        int recb = ServerStream.Read(inStream, 0, Client.ReceiveBufferSize);
                        if (recb == 0) return;

                        string returndata = System.Text.Encoding.ASCII.GetString(inStream);

                        if (returndata.Length == 0)
                        {
                            continue;
                        }

                        cmd = new DataPacket();
                        //cmd = JsonConvert.DeserializeObject<DataPacket>(returndata);

                        if (cmd.SenderID != string.Empty)
                        {
                            if(Metadata.InputDictionary.Where(_ => _.Name.Equals(cmd.Message)).Count() == 0 )
                            {
                                // send invalid cmd response to server
                                continue;
                            }
                            FormRef.SendManualCommand(cmd.Message);
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error while receiving. " + ex.Message);
                    }
                }

                Logger.Log("Server connection lost!");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        public void SendMonitoringResponseToServer(string response)
        {
            try
            {
                DataPacket deviceResponse = new DataPacket();

                deviceResponse.SenderID = AppID;
                deviceResponse.SenderType = this.Type;

                deviceResponse.RecepientID = ServerID;
                deviceResponse.RecepientType = AppType.Server;

                deviceResponse.Type = PacketType.MonitoringData;

                deviceResponse.Message = response;
                deviceResponse.TimeStamp = DateTime.Now;

                SendDataToServer(deviceResponse);
            }
            catch(Exception ex)
            {
                Logger.Log("Unable to SendMonitoringResponseToServer " + ex.Message);
            }

        }

        public void SendToServer_Manual(string strData)
        {
            try
            {
                DataPacket deviceResponse = new DataPacket();

                deviceResponse.SenderID = AppID;
                deviceResponse.SenderType = this.Type;

                deviceResponse.RecepientID = ServerID;
                deviceResponse.RecepientType = AppType.Server;

                deviceResponse.Type = PacketType.Request;

                deviceResponse.Message = strData;
                deviceResponse.TimeStamp = DateTime.Now;

                SendDataToServer(deviceResponse);
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to SendMonitoringResponseToServer " + ex.Message);
            }

        }

        public bool SendDataToServer(DataPacket dpDataToSend)
        {
            bool IsSent = false;
            try
            {
                string data = string.Empty;
                data = JsonConvert.SerializeObject(dpDataToSend) + "|";

                data = JsonConvert.SerializeObject(dpDataToSend);
                byte[] ByteData = Encoding.ASCII.GetBytes(data + "|");


                if (Client.Connected && ServerStream.CanWrite)
                {

                    ServerStream.Write(ByteData, 0, ByteData.Length);
                    ServerStream.Flush();
                    IsSent = true;
                    Logger.Log(AppID + " : " + dpDataToSend.Message);
                }
                else
                {
                    Logger.Log("Server NA / Non-Writable on SendDataToServer ");
                    Logger.Log("Server " + Client.Connected + " / " + ServerStream.CanWrite + " on SendDataToServer");
                }



            }
            catch (Exception sendEx)
            {
                Logger.Log("SendDataToServer Err" + sendEx.Message.ToString());
            }

            return IsSent;
        }
    }
}
