using Denyo.ConnectionBridge.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        //System.Net.Sockets.Socket clientSocket = null;

        BackgroundWorker bwReceiver = new BackgroundWorker();
        NetworkStream serverStream = null;
        string MsgFromServer = string.Empty;
        static bool HeartBeatRequired = false;

        string AppID = "TestClient01";
        string ServerID = string.Empty;
        string AuthToken = string.Empty;

        AppType myType = AppType.Client;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                if (textBox2.Text == string.Empty) return;

                if(!serverStream.CanWrite) { msg("Writing to server not possible at this moment!"); }
                //NetworkStream serverStream = clientSocket.GetStream();
                //NetworkStream serverStream = new NetworkStream(clientSocket);
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox2.Text + "$");

                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
                
                textBox2.Text = "";

                textBox2.Focus();

            }
            catch (Exception ex)
            {
                msg( " Error while posting msg to server. " + ex.Message);
            }
}
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StartClient()
        {
            bool bSend, bReceive, bPacketSend, bPacketReceive;
            bSend = bReceive = bPacketSend = bPacketReceive = false;
            string data = "";
            byte[] ByteData;

            DataPacket dpTest = new DataPacket();

            AuthToken = "";ServerID = "";
            try
            {
                msg("Client Started");

                //this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                this.clientSocket.Connect(this.serverEP);

                clientSocket.Connect(
                    Microsoft.VisualBasic.Interaction.InputBox("Enter Server IP to connect", "Server Params #1", "localhost", -1, -1) //13.58.37.241
                    , int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Enter Server Port connect", "Server Params #2", "9000", -1, -1)));
                //clientSocket.Connect("localhost", 8888);

                msg("Client Socket Program - Server Connected ...");
                serverStream = clientSocket.GetStream();

                msg("Connection Negotiator Initiated For Server ");

                DateTime dtTimeStageStarted = DateTime.Now;
                TimeSpan NegotiateStageTime, NegotiateTotalTime;

                NegotiateStageTime = new TimeSpan(0, 0, 0, 1, 200);
                NegotiateTotalTime = new TimeSpan(0, 0, 10, 1);

                DateTime dtTimeNegEndTime = DateTime.Now + NegotiateTotalTime;
                int PresentStageId = 0; bool IsPresentStageCompleted = false;
                int AttemptCount = 0;

                //bool bSend, bReceive, bPacketSend, bPacketReceive;
                bSend = bReceive = bPacketSend = bPacketReceive = false;

                //byte[] ByteData; string data = "";
                //DataPacket dpTest = new DataPacket(); 
                List<DataPacket> dpTestPackets = new List<DataPacket>();

                msg("Connection Negotiator Server Start "+" StT: " + DateTime.Now.ToString("HH:mm:ss:fff") + " - EdT: " + dtTimeNegEndTime.ToString("HH:mm:ss:fff"));

                while ((DateTime.Now <= dtTimeNegEndTime) && string.IsNullOrEmpty(AuthToken))
                {
                    switch (PresentStageId)
                    {
                        case 0: // ReceiveTest (New)

                            #region Init_Receive_Test

                            msg("Init_Receive_Test Started #" + AttemptCount);

                            try
                            {
                                msg("Server " + clientSocket.Connected + " / " + serverStream.DataAvailable + " / " + clientSocket.Available+ " on Init_Receive_Test @ " + AttemptCount);

                                if (clientSocket.Connected && serverStream.DataAvailable)
                                {

                                    ByteData = new byte[clientSocket.ReceiveBufferSize];
                                    int recb = serverStream.Read(ByteData, 0, clientSocket.ReceiveBufferSize);

                                    if (recb == 0)
                                    {
                                        msg("Init_Receive_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                        continue;
                                    }
                                    else
                                        msg("Init_Receive_Test DataCount " + recb + " Length @ #: " + AttemptCount);

                                    data = System.Text.Encoding.ASCII.GetString(ByteData);

                                    if (data.Length > 0)
                                    {
                                        bReceive = true;
                                        msg("Init_Receive_Test Success @ #: " + AttemptCount + " D: " + data);

                                        PresentStageId++;
                                        AttemptCount = 0;
                                    }
                                    else
                                    {
                                        msg("Init_Receive_Test Data 0 Length @ #: " + AttemptCount);
                                    }
                                }
                                else
                                {
                                    msg("Server NA / ND on Init_Receive_Test @ " + AttemptCount);
                                    msg("Server " + clientSocket.Connected + " / " + serverStream.DataAvailable + " on Init_Receive_Test @ " + AttemptCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                msg("Init_Receive_Test #" + AttemptCount + " Err: " + ex.Message);
                            }

                            if (!bReceive)
                            {
                                msg("Init_Receive_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 1000000; j++) { } // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_Receive_Test
                            break;

                        case 1: // Write Test (0-ReceiveTest)

                            #region Init_Send_Test

                            msg("Init_Send_Test Started #" + AttemptCount);

                            try
                            {
                                ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                                if (clientSocket.Connected && serverStream.CanWrite)
                                {

                                    serverStream.Write(ByteData, 0, ByteData.Length);
                                    serverStream.Flush();

                                    msg("Init_Send_Test Success @ #" + AttemptCount);
                                    bSend = true;
                                    PresentStageId++; AttemptCount = 0;
                                }
                                else
                                {
                                    msg("Server NA / Non-Writable on Init_Send_Test @ " + AttemptCount);
                                    msg("Server " + clientSocket.Connected + " / " + serverStream.CanWrite + " on Init_Send_Test @ " + AttemptCount);
                                }


                            }
                            catch (Exception ex)
                            {
                                msg("Init_Send_Test Error on #" + AttemptCount + " Error: " +ex.Message);
                            }

                            if (!bSend)
                            {
                                msg("Init_Send_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 4000000; j++) {} // give a small delay
                                AttemptCount++;
                            }

                            #endregion
                            break;

                        case 2: // ReceivePacket (0-Write-Completed, 1-Receive-Completed, 2-SendPacketCompleted)

                            #region Init_ReceivePacket_Test

                            msg("Init_ReceivePacket_Test Started #" + AttemptCount);

                            try
                            {

                                msg("Server " + clientSocket.Connected + " / " + clientSocket.Available + " / " + serverStream.DataAvailable + "  on Init_ReceivePacket_Test @ " + AttemptCount);

                                if (clientSocket.Connected && clientSocket.Available > 0)
                                {

                                    ByteData = new byte[clientSocket.ReceiveBufferSize];
                                    int recb = serverStream.Read(ByteData, 0, clientSocket.ReceiveBufferSize);
                                    if (recb == 0)
                                    {
                                        msg("Init_ReceivePacket_Test DataCount " + recb + " Length @ #: " + AttemptCount);
                                        continue;
                                    }
                                    else
                                        msg("Init_ReceivePacket_Test DataCount " + recb + " Length @ #: " + AttemptCount);

                                    data = System.Text.Encoding.ASCII.GetString(ByteData);

                                    if (data.Length > 0)
                                    {
                                        //bPacketReceive = true;
                                        msg("Init_ReceivePacket_Test Success @ #: " + AttemptCount + " D: " + data);

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
                                                        msg("Init_ReceivePacket_Test Success #" + AttemptCount);

                                                        PresentStageId++;
                                                        AttemptCount = 0;

                                                    }
                                                    else
                                                        msg("Init_ReceivePacket_Test OneParse Err SenderID Empty");
                                                }
                                                catch (Exception OneParseEx)
                                                {
                                                    msg("Init_ReceivePacket_Test OneParseEx #" + OneParseEx.Message);
                                                }
                                            }
                                        }
                                        catch (Exception ParseEx)
                                        {
                                            msg("Init_ReceivePacket_Test ParseEx #" + ParseEx.Message);
                                        }

                                    }
                                    else
                                    {
                                        msg("Init_ReceivePacket_Test Data 0 Length @ #: " + AttemptCount);
                                    }
                                }
                                else
                                {
                                    msg("Server NA / ND on Init_ReceivePacket_Test @ " + AttemptCount);
                                    msg("Server " + clientSocket.Connected + " / " + clientSocket.Available + " on Init_ReceivePacket_Test @ " + AttemptCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                msg("Init_ReceivePacket_Test #" + AttemptCount + " Err" + ex.Message);
                            }

                            if (!bPacketReceive)
                            {
                                msg("Init_ReceivePacket_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 1000000; j++) { } // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_ReceivePacket_Test
                            break;

                        case 3: // SendPacket (0-Write-Completed, 1-Receive-Completed)

                            #region Init_SendPacket_Test
                            msg("Init_SendPacket_Test Started @ #" + AttemptCount);

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

                                if (clientSocket.Connected && serverStream.CanWrite)
                                {

                                    serverStream.Write(ByteData, 0, ByteData.Length);
                                    serverStream.Flush();

                                    msg("Init_SendPacket_Test Success @ #" + AttemptCount);
                                    bPacketSend = true;
                                    PresentStageId++; AttemptCount = 0;
                                }
                                else
                                {
                                    msg("Server NA / Non-Writable on Init_SendPacket_Test @ " + AttemptCount);
                                    msg("Server " + clientSocket.Connected + " / " + serverStream.CanWrite + " on Init_SendPacket_Test @ " + AttemptCount);
                                }


                            }
                            catch (Exception ex)
                            {
                                msg("Init_SendPacket_Test #" + AttemptCount + " Err: "+ex.Message );
                            }


                            if (!bPacketSend)
                            {
                                msg("Init_SendPacket_Test Failed @ #" + AttemptCount);
                                for (int j = 0; j <= 4000000; j++) {} // give a small delay
                                AttemptCount++;
                            }


                            #endregion Init_SendPacket_Test
                            break;

                        default:
                            msg("Unknown Stage ID" + PresentStageId);
                            break;
                    }

                    //(0 - Write - Completed, 1 - Receive - Completed, 2 - SendPacketCompleted, 3 - ReceivePacket)
                    if (bSend && bReceive && bPacketReceive && bPacketSend)
                        AuthToken = ServerID;
                    else
                        System.Threading.Thread.Sleep(100);

                }

                msg("Connection Negotiator Ended For Server" );

                if (!string.IsNullOrEmpty(AuthToken))
                {
                    msg("Client Initiated Successfully to " + ServerID);
                }
                else
                {
                    msg("Client Initiation UnSuccessfull " + ServerID+ " [ " + bSend + bReceive + bPacketSend + bPacketReceive + " ] ");
                }




                /*

                msg(" IRT ");

                #region Init_Receive_Test

                msg("Init_Receive_Test");

                string returndata="";


                if (clientSocket.Connected)
                {
                    try
                    {
                        if (!serverStream.DataAvailable)
                        {
                            msg("ND on Init_Receive_Test #1");
                        }
                        else
                        {
                            byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                            int recb = serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                            if (recb == 0) return;

                            returndata = System.Text.Encoding.ASCII.GetString(inStream);
                            returndata = returndata.Substring(0, returndata.IndexOf("$"));
                            msg("Success on Init_Receive_Test #1");
                            msg(returndata);

                            bReceive = (returndata.Length > 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        msg("Error Init_Receive_Test. " + ex.Message);
                    }

                }
                else
                    msg("Init_Receive_Test Server NA");

                if(!bReceive)
                {
                    for (int j = 0; j <= 999999000; j++) {} // give a small delay

                    if (clientSocket.Connected)
                    {
                        try
                        {
                            if (!serverStream.DataAvailable)
                            {
                                msg("ND on Init_Receive_Test #2");
                            }
                            else
                            {
                                byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                                int recb = serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                                if (recb == 0) return;

                                returndata = System.Text.Encoding.ASCII.GetString(inStream);
                                returndata = returndata.Substring(0, returndata.IndexOf("$"));
                                msg("Success on Init_Receive_Test #2");
                                msg(returndata);

                                bReceive = (returndata.Length > 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            msg("Error Init_Receive_Test. " + ex.Message);
                        }

                    }
                    else
                        msg("Init_Receive_Test Server NA");
                }

                

                msg("End_Receive_Test");
                #endregion

                msg(" IST ");

                #region Init_Send_Test

                ByteData = Encoding.ASCII.GetBytes(DateTime.Now.ToString("dd MM yyyy HH mm ss") + "$");

                
                try
                {
                    if (!clientSocket.Connected || !serverStream.CanWrite)
                    {
                        msg("Writing to server not possible at this moment! Init_Send_Test #1");
                    }
                    else
                    {
                        serverStream.Write(ByteData, 0, ByteData.Length);
                        serverStream.Flush();

                        bSend = true;
                    }
                }
                catch(Exception ex)
                {
                    msg("Error on Init_Send_Test #1" + ex.Message);
                }

                if (!bSend)
                {
                    for (int j = 0; j <= 490000000; j++) {} // give a small delay

                    try
                    {
                        if (!clientSocket.Connected || !serverStream.CanWrite)
                        {
                            msg("Writing to server not possible at this moment! Init_Send_Test #2");
                        }
                        else
                        {
                            serverStream.Write(ByteData, 0, ByteData.Length);
                            serverStream.Flush();

                            bSend = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        msg("Error on Init_Send_Test #2" + ex.Message);
                    }
                }



                #endregion

                msg(" IRPT ");

                #region Init_ReceivePacketTest


                try
                {
                    if (clientSocket.Connected && clientSocket.Available > 0)
                    {

                        ByteData = new byte[clientSocket.ReceiveBufferSize];
                        int recb = serverStream.Read(ByteData, 0, clientSocket.ReceiveBufferSize);

                        if (recb == 0) { msg("Init_ReceivePacket_Test 1: ND"); }

                        data = System.Text.Encoding.ASCII.GetString(ByteData);

                        if (!(data.Length > 0))
                        {
                            bPacketReceive = false;
                            msg("Init_ReceivePacket_Test 1: ND");


                            dpTest = new DataPacket();
                            dpTest = JsonConvert.DeserializeObject<DataPacket>(data);

                            msg("Init_ReceivePacket_Test: 1 :" + data);
                            if (dpTest.SenderID != string.Empty)
                            {
                                this.ServerID = dpTest.SenderID;
                                bPacketReceive = true;
                                msg("Init_ReceivePacket_Test: 1 : Success");
                            }
                        }
                    }
                    else
                        msg("Init_ReceivePacket_Test 1 : Server NA");

                }
                catch(Exception ex)
                {
                    bPacketReceive = false;
                    msg("Init_ReceivePacket_Test: 1 Error " + ex.Message);
                }

                //Attempt #2
                if (!bPacketReceive)
                {
                    for (int j = 0; j <= 999999000; j++) {} // give a small delay

                    try
                    {
                        if (clientSocket.Connected && clientSocket.Available > 0)
                        {

                            ByteData = new byte[clientSocket.ReceiveBufferSize];
                            int recb = serverStream.Read(ByteData, 0, clientSocket.ReceiveBufferSize);

                            if (recb == 0) { msg("Init_ReceivePacket_Test 2: ND"); }

                            data = System.Text.Encoding.ASCII.GetString(ByteData);

                            if (!(data.Length > 0))
                            {
                                bPacketReceive = false;
                                msg("Init_ReceivePacket_Test 2: ND");


                                dpTest = new DataPacket();
                                dpTest = JsonConvert.DeserializeObject<DataPacket>(data);

                                msg("Init_ReceivePacket_Test: 2 : " + data);
                                if (dpTest.SenderID != string.Empty)
                                {
                                    this.ServerID = dpTest.SenderID;
                                    bPacketReceive = true;
                                    msg("Init_ReceivePacket_Test: 2 : Success" );
                                }
                            }
                        }
                        else
                            msg("Init_ReceivePacket_Test 2: Server NA");

                    }
                    catch (Exception ex)
                    {
                        bPacketReceive = false;
                        msg("Init_ReceivePacket_Test: 2 Error " + ex.Message);
                    }
                }

                #endregion

                msg(" ISPT ");

                #region Init_SendPacket_Test


                dpTest = new DataPacket();
                dpTest.Message = "Client Test Data";
                dpTest.SenderID = AppID;
                dpTest.SenderType = myType;
                dpTest.RecepientID = ServerID;
                dpTest.RecepientType = AppType.Server;
                dpTest.TimeStamp = DateTime.Now;

                try
                {
                    data = JsonConvert.SerializeObject(dpTest);
                    ByteData = Encoding.ASCII.GetBytes(data);

                    if (clientSocket.Connected && serverStream.CanWrite)
                    {
                        msg("Init_SendPacket_Test: 1: " + data);
                        serverStream.Write(ByteData, 0, ByteData.Length);
                        serverStream.Flush();

                        bPacketSend = true;
                        msg("Init_SendPacket_Test: 1: Success");
                    }
                    else
                        msg("Init_SendPacket_Test: 1: Server NA");
                }
                catch (Exception spEx)
                {
                    bPacketSend = false;
                    msg("Init_SendPacket_Test: 1: Error : " +spEx.Message);
                }


                if (!bPacketSend)
                {
                    for (int j = 0; j <= 990000000; j++) {} // give a small delay

                    try
                    {
                        data = JsonConvert.SerializeObject(dpTest);
                        ByteData = Encoding.ASCII.GetBytes(data);

                        if (clientSocket.Connected && serverStream.CanWrite)
                        {
                            msg("Init_SendPacket_Test: 2: " + data);
                            serverStream.Write(ByteData, 0, ByteData.Length);
                            serverStream.Flush();

                            bPacketSend = true;
                            msg("Init_SendPacket_Test: 2: Success");
                        }
                        else
                            msg("Init_SendPacket_Test: 2: Server NA");
                    }
                    catch (Exception spEx)
                    {
                        bPacketSend = false;
                        msg("Init_SendPacket_Test: 2: Error : " + spEx.Message);
                    }

                }

                #endregion

                */


                msg(" Initiating Listener ");

                bwReceiver.WorkerSupportsCancellation = true;
                bwReceiver.DoWork += BwReceiver_DoWork;
                bwReceiver.RunWorkerAsync();

                msg(" Awaiting for user input ");
                textBox2.Text = @"Hi";
             //   button1_Click(null, null);

            }
            catch(Exception ex)
            {
                msg("Error on init. " + ex.Message);
            }

        }

        private void BwReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (clientSocket.Connected)
                {
                    try
                    {
                        if (!serverStream.DataAvailable) continue;

                        byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                        int recb = serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                        if (recb == 0) return;

                        string returndata = "Server: " + System.Text.Encoding.ASCII.GetString(inStream);
                        returndata = returndata.Substring(0, returndata.IndexOf("$"));

                        msg(returndata);

                    }
                    catch (Exception ex)
                    {
                        msg("Error while receiving. " + ex.Message);
                    }
                }

                msg("Server connection lost!");
            }
            catch (Exception ex)
            {
                msg(ex.Message);
            }
        }

        private void ServerWatch()
        {
            try
            {
                while (clientSocket.Connected)
                {
                    try
                    {
                        if (!serverStream.DataAvailable) continue;

                        byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                        int recb = serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                        if (recb == 0) return;

                        string returndata = "Server: " + System.Text.Encoding.ASCII.GetString(inStream);
                        returndata = returndata.Substring(0, returndata.IndexOf("$"));
                        
                        msg(returndata);
                        
                    }
                    catch(Exception ex)
                    {
                        msg("Error while receiving. " + ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                msg(ex.Message);
            }
        }

        delegate void StringArgReturningVoidDelegate(string text);

        public void msg(string mesg)

        {

            if (this.textBox1.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(msg);
                this.Invoke(d, new object[] { mesg });
            }
            else
            {
                this.textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + mesg; ;
            }
           // textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + mesg;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                StartClient();

                /*

                if (!serverStream.DataAvailable) return;

                byte[] inStream = new byte[clientSocket.ReceiveBufferSize];

                int recb = serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
                if (recb == 0) return;

                string returndata = "Server: " + System.Text.Encoding.ASCII.GetString(inStream);
                returndata = returndata.Substring(0, returndata.IndexOf("$"));
                textBox3.Text = returndata;

                */
            }
            catch(Exception ex)
            {

            }
        }
    }
}

