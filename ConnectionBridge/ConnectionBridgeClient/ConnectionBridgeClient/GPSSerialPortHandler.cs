using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Client
{
    public class GPSSerialPortHandler
    {
        public object Ref { get; set; }

        public Main FormRef { get; set; }

        public Main_noUI objNonUIRef { get; set; }

        private SerialPort GPSSerialPort;

        private int _baudRate { get; set; }

        private int _dataBits { get; set; }

        private StopBits _stopBits { get; set; }

        private Parity _parity { get; set; }

        private string _portName { get; set; }

        private bool GotResponseForPrevCmd { get; set; }

        private bool CommandSent { get; set; }

        private CommunicationMode _mode { get; set; }

        private int waitCounter { get; set; }

        private Tuple<string, CommunicationMode, string> CurrentCmd;

        private Tuple<string, CommunicationMode, string> SentCmd;

        public DateTime CmdSentTime = DateTime.Now;

        public DateTime CmdReceivedTime = DateTime.Now;

        string strTmpRequest, strTmpResponse;

        public bool _sending;

        public bool _receiving;

        public Queue<Tuple<string, CommunicationMode, string>> SentQueue = new Queue<Tuple<string, CommunicationMode, string>>();

        public bool IsConnected { get; set; }

        Stopwatch RunHrWait = new Stopwatch();

        string _receivedcmd = string.Empty;
        int _minHexaLength = 0;
        int _WaitTime = 50;

        public GPSSerialPortHandler()
        {
            try {
                IsConnected = false;
                GPSSerialPort = new SerialPort();
                _baudRate = int.Parse(ConfigurationManager.AppSettings["GPSBaud"]);
                _dataBits = int.Parse(ConfigurationManager.AppSettings["GPSDataBit"]);
                _stopBits = (StopBits)Enum.Parse(typeof(StopBits), ConfigurationManager.AppSettings["GPSStopBit"]);
                _parity = (Parity)Enum.Parse(typeof(Parity), ConfigurationManager.AppSettings["GPSParity"]); ;
                _portName = ConfigurationManager.AppSettings["GPSPort"];
                GPSSerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceiver);
                GotResponseForPrevCmd = true;
                _mode = CommunicationMode.HEXA;
                OpenConnection();
            }
            catch(Exception ex)
            {
                UpdateLogWindow("Init GPSSerialPortHandler Err." + ex.Message);
            }
        }
        private void OpenConnection()
        {
            try
            {
                UpdateLogWindow("GPS Serial Initialization");

                if (GPSSerialPort.IsOpen)
                {
                    GPSSerialPort.Close();
                }
                GPSSerialPort.BaudRate = _baudRate;
                GPSSerialPort.DataBits = _dataBits;
                GPSSerialPort.StopBits = _stopBits;
                GPSSerialPort.Parity = _parity;
                GPSSerialPort.PortName = _portName;
                GPSSerialPort.DtrEnable = true;
                GPSSerialPort.RtsEnable = true;


                GPSSerialPort.Open();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                UpdateLogWindow("GPS Serial OpenConnection Err." + ex.Message);
                throw;
            }
        }

        public void SendNextCommand(string cmd, CommunicationMode mode)
        {
            try
            {
                _mode = mode;
                DataSender(cmd, mode,
                    new Tuple<string, CommunicationMode, string>(cmd, mode, "GPS," + Metadata.InputDictionaryCollection["GPS"].FirstOrDefault(x => x.Hexa == cmd).Name)
                    );
                CurrentCmd = new Tuple<string, CommunicationMode, string>(cmd, mode, "GPS," + Metadata.InputDictionaryCollection["GPS"].FirstOrDefault(x => x.Hexa == cmd).Name);
                GotResponseForPrevCmd = false;

            }
            catch (Exception ex)
            {
                UpdateLogWindow("SendNextCommand Err. " + ex.Message);
            }
        }

        private void DataSender(string cmd, CommunicationMode mode, Tuple<string, CommunicationMode, string> CommandToSend)
        {
            try
            {
                _sending = true;
                if (string.IsNullOrEmpty(cmd) || string.IsNullOrWhiteSpace(cmd))
                    return;
                else
                    cmd = cmd.Trim();

                switch (CommandToSend.Item2)
                {
                    case CommunicationMode.TEXT:
                        GPSSerialPort.Write(CommandToSend.Item1);
                        CmdSentTime = DateTime.Now;
                        CommandSent = true;
                        SentQueue.Enqueue(CommandToSend);
                        UpdateLogWindow("GPS Data Sent: " + CommandToSend.Item1 + " , " + CommandToSend.Item3);
                        break;

                    case CommunicationMode.HEXA:
                        try
                        {
                            byte[] buffer = HexToByte(CommandToSend.Item1);
                            GPSSerialPort.Write(buffer, 0, buffer.Length);
                            CmdSentTime = DateTime.Now;
                            CommandSent = true;
                            SentQueue.Enqueue(CommandToSend);
                            UpdateLogWindow("GPS Data Sent: " + CommandToSend.Item1 + " , " + CommandToSend.Item3);
                        }
                        catch (Exception ex)
                        {
                            UpdateLogWindow("GPS DataSender failed:" + ex.Message);
                            CommandSent = false;
                            Logger.Log("GPS DataSender: Failed.", ex);
                            throw;
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                UpdateLogWindow("GPS DataSender Err" + ex.Message + " Data: " + (string.IsNullOrEmpty(cmd) ? " " : cmd));
            }
            finally
            {
                //FormRef.timer1.Enabled = true;
                _sending = false;
            }
        }

        private byte[] HexToByte(string cmd)
        {
            cmd = cmd.Replace(" ", "");
            byte[] buffer = new byte[cmd.Length / 2];
            for (int i = 0; i < cmd.Length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(cmd.Substring(i, 2), 0x10);
            }
            return buffer;
        }

        private void DataReceiver(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                UpdateLogWindow("GPS DataReceiver");

                var SentCmd = SentQueue.Dequeue();
                _receiving = true;

                GotResponseForPrevCmd = true;

                if (ConfigurationManager.AppSettings["UI_ENABLED"] != null && ConfigurationManager.AppSettings["UI_ENABLED"].ToString().ToLower() == "true")
                    Main.GPSCmdCounter++;
                else
                    Main_noUI.GPSCmdCounter++;
                
                string response = string.Empty;

                switch (SentCmd.Item2)
                {
                    case CommunicationMode.TEXT:
                        response = GPSSerialPort.ReadExisting();
                        CmdReceivedTime = DateTime.Now;
                        break;

                    case CommunicationMode.HEXA:
                        {
                            _receivedcmd = SentCmd.Item3.Split(',')[1];
                            int bytesToRead = GPSSerialPort.BytesToRead;
                            RunHrWait.Restart();
                            while ((response.Trim().Split(" ".ToCharArray()).Length < _minHexaLength || bytesToRead > 0) && RunHrWait.ElapsedMilliseconds < _WaitTime)
                            {
                                byte[] buffer2 = new byte[bytesToRead];
                                GPSSerialPort.Read(buffer2, 0, bytesToRead);
                                CmdReceivedTime = DateTime.Now;
                                response += ByteToHex(buffer2);
                                bytesToRead = GPSSerialPort.BytesToRead;
                            }
                            RunHrWait.Stop();
                            break;
                        }
                }

                if (!string.IsNullOrEmpty(response))
                    response = DataStructures.Converter.GPSHexaToString(response);

                if (!CommandSent)
                {
                    if (ConfigurationManager.AppSettings["UI_ENABLED"] != null && ConfigurationManager.AppSettings["UI_ENABLED"].ToString().ToLower() == "true")
                        UpdateLogWindow("[ Request: " + SentCmd.Item3 + " ][ Response: " + response + " ] [NOTSAVED][" + Main.GPSCmdCounter.ToString() + "]");
                    else
                        UpdateLogWindow("[ Request: " + SentCmd.Item3 + " ][ Response: " + response + " ] [NOTSAVED][" + Main_noUI.GPSCmdCounter.ToString() + "]");

                    return;
                }// TO avoid saving response if a command is considered to be waited enough with no response


                if (ConfigurationManager.AppSettings["UI_ENABLED"] != null && ConfigurationManager.AppSettings["UI_ENABLED"].ToString().ToLower() == "true")
                {
                    UpdateLogWindow("[ Request: " + SentCmd.Item3 + " ][ Response: " + response + " ][" + Main.GPSCmdCounter.ToString() + "][" + SentQueue.Count + "][" + RunHrWait.ElapsedMilliseconds + "]");

                    if (Main.GPSCmdCounter >= Metadata.InputDictionaryCollection["GPS"].Count)
                    {
                        Main.GPSCmdCounter = 0;
                        UpdateLogWindow("[ Saving : " + "GPS," + response);
                        response = SentCmd.Item3 + "," + (response.Substring(response.IndexOf(":") + 2, response.IndexOf("OK") - response.IndexOf(":") - 6).Replace(",", "~"));
                        SaveGPSResponse(response);
                        UpdateLogWindow("Saved " + response);
                    }
                    else
                    {
                        FormRef.ProcessGPSCommands();
                    }
                }
                else
                {
                    UpdateLogWindow("[ Request: " + SentCmd.Item3 + " ][ Response: " + response + " ][" + Main_noUI.GPSCmdCounter.ToString() + "][" + SentQueue.Count + "][" + RunHrWait.ElapsedMilliseconds + "]");

                    if (Main_noUI.GPSCmdCounter >= Metadata.InputDictionaryCollection["GPS"].Count)
                    {
                        Main_noUI.GPSCmdCounter = 0;
                        UpdateLogWindow("[ Saving : " + "GPS," + response);
                        response = SentCmd.Item3 + "," + (response.Substring(response.IndexOf(":") + 2, response.IndexOf("OK") - response.IndexOf(":") - 6).Replace(",", "~"));
                        SaveGPSResponse(response);
                        UpdateLogWindow("Saved " + response);
                    }
                    else
                    {
                        objNonUIRef.ProcessGPSCommands();
                    }

                }

                
            }
            catch (Exception ex)
            {
                UpdateLogWindow("GPS Receiver Error:" + ex.Message + "\n");
            }
            finally
            {
                _receiving = false;
            }
        }

        public void StopAll()
        {
            try
            {

                if (GPSSerialPort !=null && GPSSerialPort.IsOpen)
                {
                    GPSSerialPort.Close();
                    GPSSerialPort.DataReceived -= DataReceiver;
                }

            }
            catch(Exception ex1)
            {
                Logger.Log("GPS StopAll." + ex1.Message);
            }

            try
            {
                if (GPSSerialPort != null)
                {
                    GPSSerialPort.Dispose();
                    GPSSerialPort = null;
                }

                Logger.Log("GPS Port stopped");
            }
            catch (Exception ex1)
            {
                Logger.Log("GPS StopAll." + ex1.Message);
            }
        }

        private void SaveGPSResponse(string response)
        {
            try
            {
                if (ConfigurationManager.AppSettings["UI_ENABLED"] != null && ConfigurationManager.AppSettings["UI_ENABLED"].ToString().ToLower() == "true")
                    FormRef.SaveGPSResponse(response);
                else
                    objNonUIRef.SaveGPSResponse(response);


            }
            catch (Exception aex)
            {
                UpdateLogWindow("SaveGPSResponse Error:" + aex.Message);
            }

        }

        private void UpdateLogWindow(string log, bool init = false)
        {
            try
            {
                //if (init)
                //    FormRef.rtbDisplay.Clear();
                //FormRef.rtbDisplay.AppendText(DateTime.Now.ToString("HH:mm:ss:ffff  > ") + log);
                //FormRef.rtbDisplay.AppendText(Environment.NewLine);

                Logger.Log(log);
            }
            catch (Exception ex)
            {

            }
        }

        private string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte num in comByte)
            {
                builder.Append(Convert.ToString(num, 0x10).PadLeft(2, '0').PadRight(3, ' '));
            }
            return builder.ToString().ToUpper();
        }

    }
}
