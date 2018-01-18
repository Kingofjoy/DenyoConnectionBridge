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

namespace Denyo.ConnectionBridge.Client
{
    public partial class Main : Form
    {
        public static bool IsInternetConnected = false;

        public static bool IsServerConnected = false;

        private SerialPortHandler serialPortHandler;

        private TcpClientHandler tcpClientHandler;

        bool bInitAll;

        public static int cmdCounter = 0;
        public Main()
        {
            InitializeComponent();

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
            catch(Exception INITSRER)
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
                Logger.Log("SetPortNameValues");
                SetPortNameValues(cboPort);
                SetParityValues(cboParity);
                SetStopBitValues(cboStop);
                SetDataBitValues(cboData);
                SetBaudRateValues(cboBaud);
                lblDevice.Text = Metadata.AppID;
                lblRemoteServer.Text = Metadata.ServerIP;
                timer1.Interval = Metadata.TimerInterval;
                if(bInitAll) timer1.Enabled = true;
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

            }
            catch (Exception imEx1)
            {

            }

            try
            {
                string HexaConfigFile = ConfigurationManager.AppSettings["HexaDictionary"];
                if (string.IsNullOrEmpty(HexaConfigFile))
                {
                    MessageBox.Show("Unable to find HexaConfig");
                    Logger.Log("Unable to find HexaConfig");
                    //Application.Exit();
                    //throw new Exception("Unable to find HexaConfig");
                }

                if (!System.IO.File.Exists(HexaConfigFile))
                {
                    MessageBox.Show("Unable to find Hexa Config File");
                    //Application.Exit(); //test
                    Logger.Log("Unable to find Hexa Config File");
                }

                foreach (string strlineitem in System.IO.File.ReadLines(HexaConfigFile))
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

                        Metadata.InputDictionary.Add(hIN);
                    }
                    catch { }
                }
            }
            catch (Exception imEx2)
            {

            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Logger.FormRef = this;
                UpdateForm();
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
            if (!bInitAll)
                InitAll();

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
        }

        private void InitAll()
        {
            try
            {
                timer1.Enabled = false;

                InitializeMetaData();

                IsInternetConnected = CheckForInternetConnection();

                InitializeFormParams();

                InitializeTcpClientHandler();
                IsServerConnected = tcpClientHandler.IsServerConnected;

                InitializeSerialPort();

                timer1.Enabled = true;
            }
            catch(Exception ex)
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
                lblRemoteServer.Text = tcpClientHandler.ServerID;
                lblInternet.Text = IsInternetConnected ? "Connected" : "Not Connected";
                lblTimer.Text = timer1.Enabled ? "ON" : "OFF";
                lblTime.Text = DateTime.Now.ToString();
                lblHC.Text = Metadata.InputDictionary.Count.ToString();
            }
            catch(Exception e) { }
        }
        
        public void SendManualCommand(string cmd)
        {
            serialPortHandler.SendManualCommand(cmd);
        }

        public void SaveResponse(string response)
        {
            tcpClientHandler.SendResponseToServer(response);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                tcpClientHandler.SendResponseToServer(txtSend.Text);
                txtSend.Clear();
            }
            catch(Exception ex)
            {

            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcpClientHandler.SendResponseToServer(txtSend.Text);
        }
    }
}
