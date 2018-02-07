using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Denyo.ConnectionBridge.DataStructures;
using System.Net.NetworkInformation;
using System.IO.Ports;
using Microsoft.Win32;
using System.Reflection;
using System.IO;

namespace Denyo.ConnectionBridge.Client
{
    public partial class Main : Form
    {
        public static bool IsInternetConnected = false;

        //public static bool IsServerConnected = false;

        private SerialPortHandler serialPortHandler;

        public TcpClientHandler tcpClientHandler;

        public bool bInitAll;

        public static int cmdCounter = 0;

        public static int lastAlarmValue = 0;

        private RegistryKey registryKey;

        public bool SwapRequired;

        public string SwapTo;
        private bool _InSwapLoop;
        private bool _isEngineRunnging;

        public Main()
        {
            InitializeComponent();
            //StartUp();
        }

        private void StartUp()
        {
            try
            {
                registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey.GetValue("Connection_Bridge_Client_Startup") == null || (string)registryKey.GetValue("Connection_Bridge_Client_Startup") != Assembly.GetExecutingAssembly().Location)
                {
                    registryKey.SetValue("Connection_Bridge_Client_Startup", Assembly.GetExecutingAssembly().Location);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void InitializeTcpClientHandler()
        {
            tcpClientHandler = new TcpClientHandler();
            tcpClientHandler.FormRef = this;
            //Todo : Check Server connection

        }

        private void InitializeSerialPort()
        {
            try
            {
                if (string.IsNullOrEmpty(cboBaud.Text) || string.IsNullOrEmpty(cboData.Text) || string.IsNullOrEmpty(cboStop.Text) || string.IsNullOrEmpty(cboParity.Text) || string.IsNullOrEmpty(cboPort.Text))
                {
                    MessageBox.Show("Unable to initialize Serial Port");
                    //Environment.Exit(1);
                }
                int baudRate = int.Parse(cboBaud.Text);
                int dataBits = int.Parse(cboData.Text);
                StopBits stopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStop.Text);
                Parity parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
                string portName = cboPort.Text;
                serialPortHandler = new SerialPortHandler(baudRate, dataBits, stopBits, parity, portName);
                serialPortHandler.FormRef = this;
            }
            catch (Exception INITSRER)
            {
                Logger.Log("Unable to initialize Serial Port Err : " + INITSRER.Message);
            }
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

        private void InitializeFormParams()
        {
            try
            {
                //Logger.Log("SetPortNameValues");
                SetPortNameValues(cboPort);
                SetParityValues(cboParity);
                SetStopBitValues(cboStop);
                SetDataBitValues(cboData);
                SetBaudRateValues(cboBaud);
                lblDevice.Text = Metadata.AppID;
                lblRemoteServer.Text = Metadata.ServerIP;
                timer1.Interval = Metadata.TimerInterval;
                //if(bInitAll) timer1.Enabled = true;
                rdoHex.Checked = true;
                UpdateForm();
            }
            catch (Exception ex)
            {
                Logger.Log("Error while initializing form params. " + ex.Message);
            }
        }



        public void SetPortNameValues(ComboBox obj)
        {
            foreach (string str in SerialPort.GetPortNames())
            {
                (obj).Items.Add(str);
            }

            if (!string.IsNullOrEmpty(Metadata.PreferredCOMPort))
                cboPort.SelectedIndex = cboPort.FindString(Metadata.PreferredCOMPort);
            else
                cboPort.SelectedIndex = 0;
        }

        public void SetParityValues(ComboBox obj)
        {
            foreach (string str in Enum.GetNames(typeof(System.IO.Ports.Parity)))
            {
                (obj).Items.Add(str);
            }

            cboParity.SelectedIndex = 0;
        }

        public void SetStopBitValues(ComboBox obj)
        {
            foreach (string str in Enum.GetNames(typeof(System.IO.Ports.StopBits)))
            {
                (obj).Items.Add(str);
            }
            cboStop.SelectedIndex = 1;
        }

        public void SetDataBitValues(ComboBox obj)
        {
            cboData.SelectedIndex = 1;
        }

        public void SetBaudRateValues(ComboBox obj)
        {
            if (!string.IsNullOrEmpty(Metadata.PreferredBaudRate))
                cboBaud.SelectedIndex = cboBaud.FindString(Metadata.PreferredBaudRate);
            else
                cboBaud.SelectedIndex = 0;
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

                Metadata.ServerIP = ConfigurationManager.AppSettings["RServer"];
                Metadata.ServerPort = int.Parse(ConfigurationManager.AppSettings["RSPort"]);

                Metadata.TimerInterval = int.Parse(ConfigurationManager.AppSettings["LoopTime"]);

                Metadata.DefaultHexaSet = ConfigurationManager.AppSettings["DefaultHexaSet"].ToUpper();

                Metadata.IdleHexaSet = ConfigurationManager.AppSettings["IdleHexaSet"].ToUpper();

                Metadata.ActiveHexaSet = Metadata.DefaultHexaSet;

            }
            catch (Exception imEx1)
            {

            }

            try
            {

                Metadata.InputDictionaryCollection.Clear();

                //string HexaConfigFile = ConfigurationManager.AppSettings["HexaDictionary"];
                //string SetPointConfigFile = ConfigurationManager.AppSettings["SetPointDictionary"];

                foreach (string filePath in Directory.GetFiles(Path.GetDirectoryName(Application.ExecutablePath), "Hexa_*.hex", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        ReadHexaInputFromFile(filePath);
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            catch (Exception imEx2)
            {

            }
        }

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

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                StartUp();
                Logger.FormRef = this;
                UpdateForm();
                Process();
            }
            catch (Exception ex)
            {

            }
        }

        private void cmdSend_Click_1(object sender, EventArgs e)
        {
            //timer1.Enabled = false;
            SendManualCommand(txtSend.Text);
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.Now.Subtract(serialPortHandler.CmdSentTime).TotalMilliseconds > Metadata.TimerInterval && DateTime.Now.Subtract(serialPortHandler.CmdReceivedTime).TotalMilliseconds > Metadata.TimerInterval && !serialPortHandler._receiving)
                {
                    serialPortHandler.SentQueue.Clear();
                    Process();
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

        public void Process()
        {
            try
            {
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

                    serialPortHandler.SendNextCommand(rdoHex.Checked ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Hexa : Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet][cmdCounter].Name, rdoHex.Checked ? CommunicationMode.HEXA : CommunicationMode.TEXT);
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
        private void InitAll()
        {
            try
            {
                //timer1.Enabled = false;

                InitializeMetaData();

                IsInternetConnected = CheckForInternetConnection();

                InitializeFormParams();

                InitializeTcpClientHandler();

                InitializeSerialPort();

                timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.Log("InitAll.Err " + ex.Message);
            }
            bInitAll = true;
        }

        private void UpdateForm()
        {
            try
            {
                IsInternetConnected = CheckForInternetConnection();
                lblRemoteServer.Text = tcpClientHandler.ServerID + ((tcpClientHandler.IsServerConnected) ? "Connected" : "Not Connected");
                lblInternet.Text = IsInternetConnected ? "Connected" : "Not Connected";
                lblTimer.Text = timer1.Enabled ? "ON" : "OFF";
                lblTime.Text = DateTime.Now.ToString();

                //foreach (var x in Metadata.InputDictionaryCollection.Keys)
                //{
                //    Logger.Log(x);
                //}
                //Logger.Log("active :" + Metadata.ActiveHexaSet);
                lblHC.Text = Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count.ToString();
                lblDevice.Text = Metadata.AppID + ((serialPortHandler.IsConnected) ? " Connected" : " Not Connected");

            }
            catch (Exception e) { }
        }

        public void SendManualCommand(string cmd)
        {
            try
            {
                Logger.Log("Manual command SendManualCommand : "+cmd);
                if (!string.IsNullOrEmpty(cmd) && cmd.IndexOf(':') > 0 && cmd.Split(':')[0] == "APPCMD")
                {
                    switch (cmd.Split(':')[1])
                    {
                        case "RESTART":
                            Application.Restart();
                            break;
                        case "EXECUTE":
                            if(!string.IsNullOrEmpty(cmd.Split(':')[2]) || Metadata.InputDictionaryCollection.ContainsKey(cmd.Split(':')[2]))
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
                    }
                }
                else if(!string.IsNullOrEmpty(cmd) && cmd.IndexOf(':') > 0 && cmd.Split(':')[0] == "DEVCMD")
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
            //try {
            //    if (!string.IsNullOrEmpty(response) && response.Split(',')[1] == "ENGINESTATE" && (int.Parse(response.Split(',')[2]) < 2) && Metadata.ActiveHexaSet != Metadata.IdleHexaSet)
            //    {
            //        _isEngineRunnging = false;
            //        // Logger.Log("Engine is not running swap HexaSet To: " + _swapTo);
            //    }
            //    else if (!string.IsNullOrEmpty(response) && response.Split(',')[1] == "ENGINESTATE" && (int.Parse(response.Split(',')[2]) > 2) && Metadata.ActiveHexaSet == Metadata.IdleHexaSet)
            //    {
            //        _isEngineRunnging = true;
            //        //Logger.Log("Engine is running swap HexaSet To: " + _swapTo);
            //    }
            //}
            //catch(Exception ex)
            //{

            //}
            tcpClientHandler.SendMonitoringResponseToServer(response, IsManualCommandResponse);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //tcpClientHandler.SendToServer_Manual(txtSend.Text);
                //txtSend.Clear();
                try
                {
                    //Process();
                    Clipboard.SetText(rtbDisplay.Text);
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcpClientHandler.SendMonitoringResponseToServer(txtSend.Text);
        }
    }
}
