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
    public class SerialPortHandler
    {
        public Main FormRef { get; set; }

        private SerialPort serialPort;

        private int _baudRate { get; set; }

        private int _dataBits { get; set; }

        private StopBits _stopBits { get; set; }

        private Parity _parity { get; set; }

        private string _portName { get; set; }

        private bool GotResponseForPrevCmd { get; set; }

        private bool CommandSent { get; set; }

        private CommunicationMode _mode { get; set; }

        private int waitCounter { get; set; }

        private bool _isManualCmd { get; set; }

        private Queue<string> ManualCmdQueue;

        private Tuple<string, CommunicationMode, bool, string> CurrentCmd;
        private Tuple<string, CommunicationMode, bool, string> SentCmd;

        public DateTime CmdSentTime = DateTime.Now;

        public DateTime CmdReceivedTime = DateTime.Now;

        string strTmpRequest, strTmpResponse;

        public bool _sending;

        public bool _receiving;

        public Queue<Tuple<string, CommunicationMode, bool, string>> SentQueue = new Queue<Tuple<string, CommunicationMode, bool, string>>();

        public bool IsConnected { get; set; }

        Stopwatch RunHrWait = new Stopwatch();

        string _receivedcmd = string.Empty;
        int _minHexaLength = 0;
        int _WaitTime = 50;

        public SerialPortHandler(int BaudRate, int DataBits, StopBits StopBits, Parity Parity, string PortName)
        {
            IsConnected = false;
            serialPort = new SerialPort();
            _baudRate = BaudRate;
            _dataBits = DataBits;
            _stopBits = StopBits;
            _parity = Parity;
            _portName = PortName;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceiver);
            GotResponseForPrevCmd = true;
            _mode = CommunicationMode.HEXA;
            ManualCmdQueue = new Queue<string>();
            OpenConnection();
        }
        private void OpenConnection()
        {
            try
            {
                UpdateLogWindow("Serial Initialization");

                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.BaudRate = _baudRate;
                serialPort.DataBits = _dataBits;
                serialPort.StopBits = _stopBits;

                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;

                serialPort.Parity = _parity;
                serialPort.PortName = _portName;
                serialPort.Open();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                UpdateLogWindow("Serial OpenConnection Err." + ex.Message);
                throw;
            }
        }

        public void SendNextCommand(string cmd, CommunicationMode mode, bool IsManulalCmd = false)
        {
            try
            {
                //_mode = mode;
                if (IsManulalCmd)
                {
                    ManualCmdQueue.Enqueue(cmd);
                }
                //if (!GotResponseForPrevCmd)
                //{
                //    UpdateLogWindow("waiting for sending cmd:" + CurrentCmd.Item4 + "\n");
                //    waitCounter++;
                //    if (waitCounter > int.Parse(ConfigurationManager.AppSettings["WaitCounter"]))
                //    {

                //        CommandSent = false;
                //        waitCounter = 0;
                //        GotResponseForPrevCmd = true;
                //        UpdateLogWindow("Command Timed Out for the command:" + CurrentCmd.Item4 + " . Wait: " + waitCounter);
                //        Logger.Log("Command Timed Out for the command:" + CurrentCmd.Item4, new Exception("TimedOut Exception"));
                //        if (ManualCmdQueue.Count > 0 && CurrentCmd.Item3)
                //        {
                //            SaveResponse(cmd + "," + "Command Timed Out, No response from the device", CurrentCmd.Item3);
                //            SendNextCommand(ManualCmdQueue.Dequeue(), CommunicationMode.HEXA, true);
                //        }
                //        else
                //        {
                //            Main.cmdCounter++;

                //        }
                //    }
                //    return;
                //}
                //else
                //{
                if (ManualCmdQueue.Count > 0)
                {
                    string mCmd = ManualCmdQueue.Dequeue();
                    UpdateLogWindow("Received Manual CMD:" + mCmd + "    *****");

                    DataSender(mCmd, CommunicationMode.HEXA,
                        new Tuple<string, CommunicationMode, bool, string>(mCmd, CommunicationMode.HEXA, true, "Manual," +
                                                   ((Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count(x => x.Hexa == mCmd) > 0) ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].FirstOrDefault(x => x.Hexa == mCmd).Name : mCmd))
                                                   );

                    CurrentCmd = new Tuple<string, CommunicationMode, bool, string>(mCmd, CommunicationMode.HEXA, true, "Manual," +
                                                   ((Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count(x => x.Hexa == mCmd) > 0) ? Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].FirstOrDefault(x => x.Hexa == mCmd).Name : mCmd));
                    //to write logic to find names in alll list
                }
                else
                {
                    //LR UpdateLogWindow("Auto cmd:\n");
                    _isManualCmd = IsManulalCmd;
                    _mode = mode;
                    DataSender(cmd, mode,
                        new Tuple<string, CommunicationMode, bool, string>(cmd, mode, IsManulalCmd, Metadata.ActiveHexaSet + "," + Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].FirstOrDefault(x => x.Hexa == cmd).Name)
                        );
                    CurrentCmd = new Tuple<string, CommunicationMode, bool, string>(cmd, mode, IsManulalCmd, Metadata.ActiveHexaSet + "," + Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].FirstOrDefault(x => x.Hexa == cmd).Name);
                }
                GotResponseForPrevCmd = false;

                //}
            }
            catch (Exception ex)
            {
                UpdateLogWindow("SendNextCommand Err. " + ex.Message);
            }
        }

        public void SendManualCommand(string cmd)
        {
            ManualCmdQueue.Enqueue(cmd);
            //SendNextCommand(cmd, CommunicationMode.TEXT, true);
        }

        private void DataSender(string cmd, CommunicationMode mode, Tuple<string, CommunicationMode, bool, string> CommandToSend)
        {
            try
            {
                //FormRef.timer1.Enabled = false;
                _sending = true;
                if (string.IsNullOrEmpty(cmd) || string.IsNullOrWhiteSpace(cmd))
                    return;
                else
                    cmd = cmd.Trim();

                /* //LR
                if (Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].Count(X => X.Hexa == cmd) > 0)
                    UpdateLogWindow("sending cmd:" + Metadata.InputDictionaryCollection[Metadata.ActiveHexaSet].FirstOrDefault(x => x.Hexa == cmd).Name + " : " + cmd);
                else
                    UpdateLogWindow("sending cmd:" + cmd);
                */ //LR

                switch (CommandToSend.Item2)
                {
                    case CommunicationMode.TEXT:
                        serialPort.Write(CommandToSend.Item1);
                        CmdSentTime = DateTime.Now;
                        CommandSent = true;
                        SentQueue.Enqueue(CommandToSend);
                        UpdateLogWindow("Data Sent:T: " + CommandToSend.Item1 + " , " + CommandToSend.Item4);
                        break;

                    case CommunicationMode.HEXA:
                        try
                        {
                            byte[] buffer = HexToByte(CommandToSend.Item1);
                            serialPort.Write(buffer, 0, buffer.Length);
                            CmdSentTime = DateTime.Now;
                            CommandSent = true;
                            SentQueue.Enqueue(CommandToSend);
                            UpdateLogWindow("Data Sent:H: " + CommandToSend.Item1 + " , " + CommandToSend.Item4);
                        }
                        catch (Exception ex)
                        {
                            UpdateLogWindow("DataSender failed:" + ex.Message);
                            CommandSent = false;
                            Logger.Log("DataSender: Failed.", ex);
                            throw;
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                UpdateLogWindow("DataSender Err" + ex.Message + " Data: " + (string.IsNullOrEmpty(cmd) ? " " : cmd));
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
                var SentCmd = SentQueue.Dequeue();
                _receiving = true;
                //LR  UpdateLogWindow("DataReceiver:\n");

                GotResponseForPrevCmd = true;
                if (!SentCmd.Item3)
                {
                    Main.cmdCounter++;
                }
                string response = string.Empty;
                //LR  UpdateLogWindow("CommunicationMode:" + SentCmd.Item2 + "\n");
                //if(SentCmd.Item4 == "A" || SentCmd.Item4 == "RUNNINGHR")
                //    for (int j = 0; j <= 1000000; j++) { /* give a small delay */ }
                //else
                //    for (int j = 0; j <= 300000; j++) { /* give a small delay */ }

                switch (SentCmd.Item2)
                {
                    case CommunicationMode.TEXT:
                        response = serialPort.ReadExisting();
                        CmdReceivedTime = DateTime.Now;
                        break;

                    case CommunicationMode.HEXA:
                        {
                            _receivedcmd = SentCmd.Item4.Split(',')[1];

                            if (_receivedcmd == "RUNNINGHR")
                            {
                                _minHexaLength = 9;
                                _WaitTime = 200;
                            }
                            else if (_receivedcmd == "A")
                            {
                                _minHexaLength = 59;
                                _WaitTime = 200;
                            }
                            else
                            {
                                _minHexaLength = 7;
                                _WaitTime = 50;
                            }

                            int bytesToRead = serialPort.BytesToRead;
                            RunHrWait.Restart();
                            while ((response.Trim().Split(" ".ToCharArray()).Length < _minHexaLength || bytesToRead > 0) && RunHrWait.ElapsedMilliseconds < _WaitTime)
                            {
                                byte[] buffer2 = new byte[bytesToRead];
                                serialPort.Read(buffer2, 0, bytesToRead);
                                CmdReceivedTime = DateTime.Now;
                                response += ByteToHex(buffer2);
                                bytesToRead = serialPort.BytesToRead;
                            }
                            RunHrWait.Stop();

                            //if (!(_receivedcmd == "RUNNINGHR" || _receivedcmd == "A")) //&& !SentCmd.Item3
                            //{
                            //    int bytesToRead = serialPort.BytesToRead;
                            //    while (bytesToRead > 0 || )
                            //    {
                            //        byte[] buffer2 = new byte[bytesToRead];
                            //        serialPort.Read(buffer2, 0, bytesToRead);
                            //        CmdReceivedTime = DateTime.Now;
                            //        response += ByteToHex(buffer2);
                            //        bytesToRead = serialPort.BytesToRead;
                            //    }
                            //}
                            //else
                            //{
                            //    // For running hour and A, we will require / receive more hex pairs than usual
                            //    // For manual commands also we are not sure about the return values so we wait for more pairs -- commented
                            //    int bytesToRead = serialPort.BytesToRead;
                            //    _minHexaLength = (_receivedcmd == "RUNNINGHR") ? 9 : 59; // || SentCmd.Item3

                            //    RunHrWait.Start();
                            //    while ((response.Trim().Split(" ".ToCharArray()).Length < _minHexaLength || bytesToRead > 0) && RunHrWait.ElapsedMilliseconds < 200)
                            //    {
                            //        byte[] buffer2 = new byte[bytesToRead];
                            //        serialPort.Read(buffer2, 0, bytesToRead);
                            //        CmdReceivedTime = DateTime.Now;
                            //        response += ByteToHex(buffer2);
                            //        bytesToRead = serialPort.BytesToRead;
                            //    }
                            //    RunHrWait.Stop();
                            //    UpdateLogWindow("Waited for " + RunHrWait.ElapsedMilliseconds + ". Hex "+ response.Split(" ".ToCharArray()).Length );
                            //    RunHrWait.Reset();

                            //}
                            break;
                        }
                }

                
                if (!CommandSent)
                {
                    UpdateLogWindow("[ Request: " + SentCmd.Item4 + " ][ Response: " + response + " ] [NOTSAVED][" + Main.cmdCounter.ToString() + "]");
                    return;
                }// TO avoid saving response if a command is considered to be waited enough with no response


                UpdateLogWindow("[ Request: " + SentCmd.Item4 + " ][ Response: " + response + " ][" + Main.cmdCounter.ToString() + "][" + SentQueue.Count + "]["+ RunHrWait.ElapsedMilliseconds +"]");
                
                SaveResponse(SentCmd.Item4 + "," + response, SentCmd.Item3);

                //if(SentCmd.Item4.IndexOf("ENGINESTATE") > 0 && int.Parse(response) == 1)
                //{

                //}

                if (SentCmd.Item3)
                {
                    if (!FormRef.SwapRequired)
                    {
                        FormRef.SwapTo = Metadata.ActiveHexaSet;
                        FormRef.SwapRequired = true;
                    }
                }
                

                FormRef.Process();
            }
            catch (Exception ex)
            {
                UpdateLogWindow("Receiver Error:" + ex.Message + "\n");
            }
            finally
            {
                _receiving = false;
            }
        }

        private void SaveResponse(string response, bool IsManualCmd)
        {
            try
            {
                strTmpRequest = response.Split(",".ToCharArray())[1];
                if (strTmpRequest == "ALARMS")
                {
                    strTmpResponse = response.Split(",".ToCharArray())[2];
                    strTmpResponse = DataStructures.Converter.HexaToString(strTmpResponse, strTmpRequest);
                    if (!int.TryParse(strTmpResponse, out Main.lastAlarmValue))
                        Main.lastAlarmValue = 0;
                }
                else if (strTmpRequest == "A")
                {
                    strTmpResponse = response.Split(",".ToCharArray())[2];
                    strTmpResponse = DataStructures.Converter.HexaToString(strTmpResponse, strTmpRequest);
                    if (System.Text.RegularExpressions.Regex.IsMatch(strTmpResponse, @"^[a-zA-Z0-9 ]+$"))
                    {
                        UpdateLogWindow("*** ALARM : " + strTmpResponse + " ****");
                    }
                }

                FormRef.SaveResponse(response, IsManualCmd);
            }
            catch (Exception aex)
            {
                UpdateLogWindow("SaveResponse Error:" + aex.Message);
            }

        }

        private void UpdateLogWindow(string log, bool init = false)
        {
            try
            {
                if (init)
                    FormRef.rtbDisplay.Clear();
                FormRef.rtbDisplay.AppendText(DateTime.Now.ToString("HH:mm:ss:ffff  > ") + log);
                FormRef.rtbDisplay.AppendText(Environment.NewLine);

            }
            catch (Exception ex)
            {

            }
            
            try
            {
                if (FormRef.rtbDisplay.TextLength > 100000)
                {
                    //FormRef.rtbDisplay.AppendText(DateTime.Now.ToString("HH:mm:ss:ffff  > ") + log + "{" + FormRef.rtbDisplay.TextLength + "}");
                    FormRef.rtbDisplay.SelectAll();
                    FormRef.rtbDisplay.Clear();
                    FormRef.rtbDisplay.Text = DateTime.Now.ToString("HH:mm:ss:ffff  > ") + "Clear Disp";
                }
            }
            catch(Exception ex)
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
