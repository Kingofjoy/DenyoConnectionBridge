using Denyo.ConnectionBridge.DataStructures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Denyo.ConnectionBridge.Client
{
    public class Main_noUI
    {

        #region Declarations

        public static bool IsInternetConnected = false;

        public static string REMOTESERVERSTATE { get; private set; }
        public static string INTERNETSTATE { get; private set; }
        public static string TIMERSTATE { get; private set; }
        public static string TIMESTATE { get; private set; }
        public static string HEXACOLLECTIONSTATE { get; private set; }
        public static string DEVICESTATE { get; private set; }
        public static string DEVICEID { get; private set; }
        public static string REMOTESERVER { get; private set; }
        public static string TIMERINTERVAL { get; private set; }
        public static string HEXBIN { get; private set; }

        public static string LASTLOG { get; set; }

        //public static bool IsServerConnected = false;

        
        private System.Timers.Timer timer1=new System.Timers.Timer();

        private SerialPortHandler serialPortHandler;

        private GPSSerialPortHandler GPSserialPortHandler;

        public TcpClientHandler tcpClientHandler;

        WebAPIHandler apiHandler;

        public bool bInitAll;

        public static int cmdCounter = 0;

        public static int GPSCmdCounter = 0;

        public static int lastAlarmValue = 0;

        //private RegistryKey registryKey;

        public bool SwapRequired;

        public string SwapTo;

        private bool _InSwapLoop;

        private bool _IsEngineRunnging;

        private bool _ExecuteGPS;

        #endregion

        public Main_noUI()
        {
            try
            {
                Logger.Log("Connection Bridge Initiated");

                this.timer1.Interval = int.Parse(ConfigurationManager.AppSettings["BKP_TIMER_INTERVAL"].ToString());
                this.timer1.Elapsed += Timer1_Elapsed; //.Tick += new System.EventHandler(this.timer1_Tick_1);

                UpdateForm();
                //timer1.Enabled = true;
                //Process();

            }
            catch(Exception ex)
            {
                Logger.Log("Connection Bridge Exception while Initiating");
                Logger.Log(ex.Message);
            }
        }

        ~Main_noUI()
        {
            try
            {
                if (apiHandler != null)
                {
                    apiHandler.Stop();
                }
            }
            catch(Exception apiSTEx)
            {
                Logger.Log("API Stop" + apiSTEx.Message, apiSTEx);
            }
        }


        /// <summary>
        /// Keep realtime status indicators updated
        /// </summary>
        private void UpdateForm()
        {
            try
            {
                IsInternetConnected = CheckForInternetConnection();
                REMOTESERVERSTATE = tcpClientHandler.ServerID + ((tcpClientHandler.IsServerConnected) ? "Connected" : "Not Connected");
                INTERNETSTATE = IsInternetConnected ? "Connected" : "Not Connected";
                TIMERSTATE = timer1.Enabled ? "ON" : "OFF";
                TIMESTATE = DateTime.Now.ToString();
                Logger.WorkerHeartBeat = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local);
                //foreach (var x in Metadata.InputDictionaryCollection.Keys)
                //{
                //    Logger.Log(x);
                //}
                //Logger.Log("active :" + Metadata.ActiveHexaSet);
                HEXACOLLECTIONSTATE = Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count.ToString();
                DEVICESTATE = Metadata.AppID + ((serialPortHandler.IsConnected) ? " Connected" : " Not Connected");



            }
            catch { }


            try
            {
                Logger.AddToStatus("DEVICESTATE", string.IsNullOrEmpty(DEVICESTATE) ? "NA" : DEVICESTATE);
                Logger.AddToStatus("REMOTESERVERSTATE", string.IsNullOrEmpty(REMOTESERVERSTATE) ? "NA" : REMOTESERVERSTATE);
                Logger.AddToStatus("INTERNETSTATE", string.IsNullOrEmpty(INTERNETSTATE) ? "NA" : INTERNETSTATE);
                Logger.AddToStatus("TIMERSTATE", string.IsNullOrEmpty(TIMERSTATE) ? "NA" : TIMERSTATE);
                Logger.AddToStatus("WORKERHEARTBEAT", Logger.WorkerHeartBeat.ToString());
                Logger.AddToStatus("HEXACOLLECTIONSTATE", string.IsNullOrEmpty(HEXACOLLECTIONSTATE) ? "NA" : HEXACOLLECTIONSTATE);
                
            }
            catch(Exception ex)
            {
                Logger.Log("Error while adding State to dict." + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actual application processing logics
        /// </summary>
        public void Process()
        {
            try
            {
                if(!Logger.ThreadLife)
                {
                    StopAll();
                    return;
                }

                if (!bInitAll)
                    InitAll();

                UpdateForm();

                //swap area
                if (SwapRequired)
                {
                    Logger.Log("SWAP " + Metadata.ActiveHexaSet + " LOOP To " + SwapTo);
                    Metadata.ActiveHexaSet = SwapTo;
                    cmdCounter = 0;
                    _InSwapLoop = true;
                    SwapRequired = false;
                    //Logger.Log("ACTIVE DS " + Metadata.ActiveHexaSet);
                    //Logger.Log("Default DS " + Metadata.DefaultHexaSet);
                }
                if (_InSwapLoop && cmdCounter >= Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count)
                {
                    //Logger.Log("SWAP LOOP " + _swapTo + " completed.");
                    cmdCounter = 0;
                    Metadata.ActiveHexaSet = Metadata.DefaultHexaSet;
                    _InSwapLoop = false;
                }

                //if(_ExecuteGPS)
                //{
                //    ProcessGPSCommands();
                //}

                //if (_isEngineRunnging && !_InSwapLoop && Metadata.ActiveHexaSet == Metadata.IdleHexaSet)
                //{
                //    cmdCounter = 0;
                //    Metadata.ActiveHexaSet = Metadata.DefaultHexaSet;
                //}

                if (IsInternetConnected && tcpClientHandler.IsServerConnected && serialPortHandler.IsConnected)
                {
                    if (cmdCounter >= Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count)
                        cmdCounter = 0;
                    while (Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Name == "A" && lastAlarmValue < 1)
                    {
                        //Logger.Log("Skipping A" + cmdCounter);
                        cmdCounter++;
                        if (_InSwapLoop && cmdCounter >= Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count)
                        {
                            //Logger.Log("A.SWAP LOOP " + _swapTo + " completed.");
                            cmdCounter = 0;
                            Metadata.ActiveHexaSet = Metadata.DefaultHexaSet;
                            _InSwapLoop = false;
                        }
                        else if (cmdCounter >= Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count)
                            cmdCounter = 0;
                    }
                    serialPortHandler.SendNextCommand((HEXBIN == "HEX") ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Hexa : Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Name, (HEXBIN == "HEX") ? CommunicationMode.HEXA : CommunicationMode.TEXT);

                    //if (!_ExecuteGPS)
                    //{
                    //    serialPortHandler.SendNextCommand(rdoHex.Checked ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Hexa : Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Name, rdoHex.Checked ? CommunicationMode.HEXA : CommunicationMode.TEXT);
                    //}
                    //else
                    //{
                    //    GPSserialPortHandler.SendNextCommand(rdoHex.Checked ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Hexa : Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Name, rdoHex.Checked ? CommunicationMode.HEXA : CommunicationMode.TEXT);
                    //    if (cmdCounter >= Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count)
                    //        _ExecuteGPS = false;
                    //}
                }
                else
                {
                    bInitAll = false;
                    timer1.Enabled = true;
                    //Save in local
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error while processing. " + ex.Message);
            }
        }

        /// <summary>
        /// When parent threat wants to stop child threat, this method should be invoked by child.
        /// </summary>
        private void StopAll()
        {
            try
            {
                timer1.Enabled = false;
                timer1.Elapsed -= Timer1_Elapsed;
            }
            catch(Exception ex)
            {
                Logger.Log("StopAll.1. " + ex.Message);
            }

            try
            {
                serialPortHandler.StopAll();
            }catch(Exception ex1)
            {
                Logger.Log("StopAll.2. " + ex1.Message);
            }

            try
            {
                GPSserialPortHandler.StopAll();
            }
            catch (Exception ex1)
            {
                Logger.Log("StopAll.3. " + ex1.Message);
            }

            try
            {
                tcpClientHandler.StopAll();
            }catch(Exception ex3)
            {
                Logger.Log("StopAll.4. " + ex3.Message);
            }
        }

        /// <summary>
        /// TO initialize all indivudual components and metadata.
        /// </summary>
        private void InitAll()
        {
            try
            {
                //timer1.Enabled = false;

                InitializeMetaData();

                IsInternetConnected = CheckForInternetConnection();

                InitializeFormParams();

                InitiateWebClientAPI();

                InitializeTcpClientHandler();

                InitializeSerialPort();

                InitializeGPSHandler();

                

                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.Log("InitAll.Err " + ex.Message);
            }
            bInitAll = true;
        }

        private bool InitiateWebClientAPI()
        {
            bool started = false;
            try
            {
                if (apiHandler == null || !apiHandler.isRunning)
                {
                    Logger.Log("Starting WebAPI...");
                    apiHandler = new WebAPIHandler();
                    started = true;
                    return apiHandler.Start();
                }
                else return true;

            }
            catch(Exception apiex)
            {
                Logger.Log("InitiateWebClientAPI Error" + apiex.Message, apiex);
                return false;
            }
            finally
            {
                if(started)
                Logger.Log("WebAPI Started");
            }
        }

        private void InitializeFormParams()
        {
            try
            {

                ////SetPortNameValues(cboPort);
                ////SetParityValues(cboParity);
                ////SetStopBitValues(cboStop);
                ////SetDataBitValues(cboData);
                ////SetBaudRateValues(cboBaud);

                DEVICEID = Metadata.AppID;
                REMOTESERVER = Metadata.ServerIP;
                TIMERINTERVAL = Metadata.TimerInterval.ToString();
                HEXBIN = "HEX";

                timer1.Interval = Metadata.TimerInterval;
                UpdateForm();
            }
            catch (Exception ex)
            {
                Logger.Log("Error while initializing form params. " + ex.Message);
            }
        }

        private void InitializeTcpClientHandler()
        {
            tcpClientHandler = new TcpClientHandler();
            tcpClientHandler.objNonUIRef = this; 

        }

        private void InitializeSerialPort()
        {
            try
            {
                string PortName=string.Empty;

                if (SerialPort.GetPortNames().Count() < 1)
                    Logger.LogFatal("NO SERIAL PORTS FOUND IN THE SYSTEM");
                else
                    PortName = SerialPort.GetPortNames().FirstOrDefault();

                if (string.IsNullOrEmpty(Metadata.PreferredCOMPort))
                    Logger.Log("Unable to initialize Serial Port AVAILABLE_SETTINGS_ERR");
                else
                    PortName = Metadata.PreferredCOMPort;

                //new object[] {"300","600","1200","1900","2400","4800","9600","19200","14400","28800","36000","115000"}
                int baudRate = 300;
                if (string.IsNullOrEmpty(Metadata.PreferredBaudRate))
                    Logger.Log("No PreferredBaudRate AVAILABLE_SETTINGS_ERR");
                else
                    baudRate = int.Parse(Metadata.PreferredBaudRate);

                // 7, 8, 9
                int dataBits = 8;

                StopBits stopBits = StopBits.One;

                Parity parity = Parity.Even;

                serialPortHandler = new SerialPortHandler(baudRate, dataBits, stopBits, parity, PortName);
                
            }
            catch (Exception INITSRER)
            {
                Logger.Log("Unable to initialize Serial Port Err : " + INITSRER.Message);
            }
        }

        private void InitializeGPSHandler()
        {
            try
            {
                GPSserialPortHandler = new GPSSerialPortHandler();
                GPSserialPortHandler.objNonUIRef = this;
                Logger.Log("GPSserialPortHandler is initialized.");
            }
            catch (Exception ex)
            {
                Logger.Log("Error while InitializeGPSHandler. " + ex.Message);
            }
        }

        private void InitializeMetaData()
        {
            try
            {
                Metadata.AppID = ConfigurationManager.AppSettings["AppID"];
                Metadata.AppType = ConfigurationManager.AppSettings["AppType"];
                Metadata.AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                Metadata.PreferredCOMPort = ConfigurationManager.AppSettings["LCPort"];
                Metadata.PreferredBaudRate = ConfigurationManager.AppSettings["LCPBaud"];

                Metadata.ATCOMPort = ConfigurationManager.AppSettings["ATPort"];

                Metadata.ServerIP = ConfigurationManager.AppSettings["RServer"];
                Metadata.ServerPort = int.Parse(ConfigurationManager.AppSettings["RSPort"]);

                Metadata.TimerInterval = int.Parse(ConfigurationManager.AppSettings["LoopTime"]);

                Metadata.DefaultHexaSet = ConfigurationManager.AppSettings["DefaultHexaSet"].ToUpper();

                Metadata.IdleHexaSet = ConfigurationManager.AppSettings["IdleHexaSet"].ToUpper();

                Metadata.ActiveHexaSet = Metadata.DefaultHexaSet;

                Metadata.DataSaverEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["DataSaverEnabled"]);

                string HexaCollection = ConfigurationManager.AppSettings["DataSaverHexaSets"];
                Logger.Log("HexColl: " + HexaCollection);

                Metadata.DataSaverHexaSet = new List<string>();
                foreach (string Hexa in HexaCollection.Split(",".ToCharArray()))
                    Metadata.DataSaverHexaSet.Add(Hexa.ToUpper());

                Logger.Log("HS C:" + Metadata.DataSaverHexaSet.Count);

                Metadata.DataSaverCacheMinutes = int.Parse(ConfigurationManager.AppSettings["DataSaverCacheMinutes"]);
            }
            catch (Exception imEx1)
            {
                Logger.Log("InitializeMetaData Err : " + imEx1.Message);
            }

            try
            {

                Metadata.InputDictionaryCollection.Clear();

                if (ConfigurationManager.AppSettings["HexaDirectory"] == null || string.IsNullOrEmpty(ConfigurationManager.AppSettings["HexaDirectory"]))
                    Logger.Log("HexaDirectory Config AVAILABLE_SETTINGS_ERR");
                else
                    Logger.Log("HexaDirectory Config: "+ ConfigurationManager.AppSettings["HexaDirectory"].ToString());

                
                foreach (string filePath in Directory.GetFiles((ConfigurationManager.AppSettings["HexaDirectory"]), "Hexa_*.hex", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        ReadHexaInputFromFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("InitializeMetaData Err ReadHexFrom("+ filePath +"): " + ex.Message);
                    }
                }

            }
            catch (Exception imEx2)
            {
                Logger.Log("InitializeMetaData Err2 : " + imEx2.Message);
            }
        }



        public void ProcessGPSCommands()
        {
            if (!Logger.ThreadLife)
            {
                StopAll();
                return;
            }

            try
            {
                if (!GPSserialPortHandler.IsConnected)
                {
                    InitializeGPSHandler();
                }
                if (GPSserialPortHandler.IsConnected)
                {
                    GPSserialPortHandler.SendNextCommand(Metadata.InputDictionaryCollection["GPS"][GPSCmdCounter].Hexa, CommunicationMode.HEXA);
                }
                else
                {
                    Logger.Log("GPSserialPortHandler is not connected.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error while GPS processing." + ex.Message);
            }
        }

        public void SendManualCommand(string cmd)
        {
            if (!Logger.ThreadLife)
            {
                StopAll();
                return;
            }

            try
            {
                Logger.Log("Manual command SendManualCommand : " + cmd);
                if (!string.IsNullOrEmpty(cmd) && cmd.IndexOf(':') > 0 && cmd.Split(':')[0] == "APPCMD")
                {
                    switch (cmd.Split(':')[1])
                    {
                        case "RESTART":
                            //Application.Restart(); ///tTODO
                            break;
                        case "EXECUTE":
                            if (!string.IsNullOrEmpty(cmd.Split(':')[2]) || Metadata.InputDictionaryCollection.ContainsKey(cmd.Split(':')[2]))
                            {
                                SwapTo = cmd.Split(':')[2];
                                SwapRequired = true;
                                //Logger.Log("SWAP RECEIVED " + _swapTo);
                            }
                            else
                            {
                                //Logger.Log("INVALID SWAP COMMAND PARAM.  Cmd: " + cmd);
                                //Logger.Log("Available SWAP Documents:");
                                //foreach(string ke in Metadata.InputDictionaryCollection.Keys)
                                //    Logger.Log(" > "+ke);
                            }
                            break;
                        case "GPS":
                            Logger.Log("GPS start: " + cmd);
                            ProcessGPSCommands();
                            Logger.Log("GPS end: " + cmd);
                            break;
                    }
                }
                else if (!string.IsNullOrEmpty(cmd) && cmd.IndexOf(':') > 0 && cmd.Split(':')[0] == "DEVCMD")
                {
                    switch (cmd.Split(':')[1])
                    {
                        case "START":
                            {
                                // 1 : 01 10 10 6F 00 02 04 01 FE 00 00 18 0b
                                // 2 : 01 06 10 71 00 01 1c d1
                                Logger.Log("ManualCommand START ENGINE");
                                serialPortHandler.SendManualCommand("01 10 10 6F 00 02 04 01 FE 00 00 18 0b");
                                serialPortHandler.SendManualCommand("01 06 10 71 00 01 1c d1");
                            }
                            break;
                        case "STOP":
                            {
                                Logger.Log("ManualCommand STOP ENGINE");
                                // 1 : 01 10 10 6f 00 02 04 02 fd 00 00 e8 4f
                                // 2 : 01 06 10 71 00 01 1c d1
                                serialPortHandler.SendManualCommand("01 10 10 6f 00 02 04 02 fd 00 00 e8 4f");
                                serialPortHandler.SendManualCommand("01 06 10 71 00 01 1c d1");
                            }
                            break;
                        default:
                            {
                                Logger.Log("ManualCommandProcessing Unable Q " + cmd);
                            }
                            break;
                    }
                }
                else
                    serialPortHandler.SendManualCommand(cmd);
            }
            catch (Exception ex)
            {
                Logger.Log("ManualCommandProcessing Error :" + ex.Message + " Cmd: " + cmd);
            }
        }

        public void SaveResponse(string response, bool IsManualCommandResponse = false)
        {
            tcpClientHandler.SendMonitoringResponseToServer(response, IsManualCommandResponse);
        }

        public void SaveGPSResponse(string response)
        {
            tcpClientHandler.SendMonitoringResponseToServer(response);
        }

        //private void timer1_Tick_1(object sender, EventArgs e)
        private void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Logger.ThreadLife)
            {
                StopAll();
                return;
            }
            try
            {
                if (DateTime.Now.Subtract(serialPortHandler.CmdSentTime).TotalMilliseconds > Metadata.TimerInterval && DateTime.Now.Subtract(serialPortHandler.CmdReceivedTime).TotalMilliseconds > Metadata.TimerInterval && !serialPortHandler._receiving)
                {
                    serialPortHandler.SentQueue.Clear();
                    Process();
                }

                //Logger.Log("last sent:" + GPSserialPortHandler.CmdSentTime.ToString());
                //Logger.Log("Diff:" + DateTime.Now.Subtract(GPSserialPortHandler.CmdSentTime).TotalMinutes);
                if (DateTime.Now.Subtract(GPSserialPortHandler.CmdSentTime).TotalMinutes > int.Parse(ConfigurationManager.AppSettings["GPSTimerInMinute"]))
                {
                    Logger.Log("GPS start: " + DateTime.Now);
                    ProcessGPSCommands();
                    Logger.Log("GPS end: " + DateTime.Now);
                }

                /*
                if (!bInitAll)
                    InitAll();

                //if(!allConnected)
                //{
                //    //// try reconnect
                //    UpdateForm();
                //    return;
                //}

                UpdateForm();
                if (IsInternetConnected && IsServerConnected)
                {
                    if (cmdCounter >= Metadata.InputDictionary.Count)
                        cmdCounter = 0;
                    serialPortHandler.SendNextCommand(rdoHex.Checked ? Metadata.InputDictionary[cmdCounter].Hexa : Metadata.InputDictionary[cmdCounter].Name, rdoHex.Checked ? CommunicationMode.HEXA : CommunicationMode.TEXT);
                }
                else
                {
                    //Save in local
                }
                */
            }
            catch (Exception ex)
            {
                Logger.Log("Backup Timer Err " + ex.Message);
            }

            
        }

        #region Utilities

        private void ReadHexaInputFromFile(string FileName)
        {
            string HexaSetName = "";
            HexaSetName = Path.GetFileName(FileName).Split("_".ToCharArray())[1];
            HexaSetName = HexaSetName.ToUpper().Replace(".HEX", "");
            //if (string.IsNullOrEmpty(FileName))
            //{
            //    MessageBox.Show("Given file is empty.");
            //    Logger.Log("Given file is empty.");
            //    //Application.Exit();
            //    //throw new Exception("Unable to find HexaConfig");
            //}

            //if (!System.IO.File.Exists(FileName))
            //{
            //    MessageBox.Show("Unable to find given Config file:" + FileName);
            //    Logger.Log("Unable to find given Config file:" + FileName);
            //    //Application.Exit(); //test
            //}

            List<HexaInput> hexList = new List<HexaInput>();
            foreach (string strlineitem in System.IO.File.ReadLines(FileName))
            {
                try
                {
                    if (string.IsNullOrEmpty(strlineitem) || string.IsNullOrWhiteSpace(strlineitem))
                        continue;
                    HexaInput hIN = new HexaInput();
                    hIN.Raw = strlineitem;
                    hIN.Hexa = strlineitem.Split(",".ToCharArray())[0];
                    hIN.Name = strlineitem.Split(",".ToCharArray())[1];
                    hIN.PX = strlineitem.Substring(0, strlineitem.Length - strlineitem.IndexOf(','));

                    hexList.Add(hIN);
                    //Metadata.InputDictionary.Add(hIN);
                }
                catch { }
            }
            if (hexList.Count > 0 && !Metadata.InputDictionaryCollection.ContainsKey(HexaSetName))
                Metadata.InputDictionaryCollection.Add(HexaSetName, hexList);
        }

        private bool CheckForInternetConnection()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else if (reply.Status == IPStatus.TimedOut)
                {
                    return IsInternetConnected;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
