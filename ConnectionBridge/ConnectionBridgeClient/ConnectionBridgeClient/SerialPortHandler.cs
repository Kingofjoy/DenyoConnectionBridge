using System;
using System.Collections.Generic;
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

        private CommunicationMode _mode { get; set; }

        private int waitCounter { get; set; }

        private bool _isManualCmd { get; set; }

        private Queue<string> ManualCmdQueue;

        private Tuple<string, CommunicationMode, bool, string> CurrentCmd;

        public SerialPortHandler(int BaudRate, int DataBits, StopBits StopBits, Parity Parity, string PortName)
        {
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
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
                serialPort.BaudRate = _baudRate;
                serialPort.DataBits = _dataBits;
                serialPort.StopBits = _stopBits;
                serialPort.Parity = _parity;
                serialPort.PortName = _portName;
                serialPort.Open();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void SendNextCommand(string cmd, CommunicationMode mode, bool IsManulalCmd = false)
        {
            //_mode = mode;
            if(IsManulalCmd)
            {
                ManualCmdQueue.Enqueue(cmd);
            }
            if (!GotResponseForPrevCmd)
            {
                waitCounter++;
                if (waitCounter > 5)
                {
                    waitCounter = 0;
                    GotResponseForPrevCmd = true;
                    Logger.Log("Command Timed Out for the command:" + CurrentCmd.Item1, new Exception("TimedOut Exception"));
                    if (ManualCmdQueue.Count > 0 && CurrentCmd.Item3)
                    {
                        SaveResponse(cmd + "," + "Command Timed Out, No response from the devicce", CurrentCmd.Item3);
                        SendNextCommand(ManualCmdQueue.Dequeue(), CommunicationMode.TEXT, true);
                    }
                    else
                    {
                        Main.cmdCounter++;
                       
                    }
                }
                return;
            }
            else
            {
                if (ManualCmdQueue.Count > 0)
                {
                    string mCmd = ManualCmdQueue.Dequeue();
                    DataSender(mCmd, CommunicationMode.TEXT);
                    CurrentCmd = new Tuple<string, CommunicationMode, bool,string>(mCmd, CommunicationMode.TEXT, true,mCmd);
                }
                else
                {
                    UpdateLogWindow("inside next cmd:\n");
                    _isManualCmd = IsManulalCmd;
                    _mode = mode;
                    DataSender(cmd, mode);
                    CurrentCmd = new Tuple<string, CommunicationMode, bool,string>(cmd, mode, IsManulalCmd,Metadata.InputDictionary.FirstOrDefault(x=>x.Hexa==cmd).Name);
                    GotResponseForPrevCmd = false;
                }
            }
        }

        public void SendManualCommand(string cmd)
        {
            SendNextCommand(cmd, CommunicationMode.TEXT, true);
        }

        private void DataSender(string cmd, CommunicationMode mode)
        {
            UpdateLogWindow("inside sender:"+ cmd+"   , " + mode.ToString() + "\n");

            switch (mode)
            {
                case CommunicationMode.TEXT:
                    serialPort.Write(cmd);
                    return;

                case CommunicationMode.HEXA:
                    try
                    {
                        byte[] buffer = HexToByte(cmd);
                        serialPort.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        UpdateLogWindow("DataSender failed:" + ex.Message);

                        Logger.Log("DataSender: Failed.", ex);
                        throw;
                    }
                    return;
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
            try {
                UpdateLogWindow("DataReceiver:\n");

                GotResponseForPrevCmd = true;
                if (!CurrentCmd.Item3)
                {
                    Main.cmdCounter++;
                }
                string response = string.Empty;
                UpdateLogWindow("CommunicationMode:" + CurrentCmd.Item2 + "\n");

                switch (CurrentCmd.Item2)
                {
                    case CommunicationMode.TEXT:
                        response = serialPort.ReadExisting();
                        break;

                    case CommunicationMode.HEXA:
                        {
                            int bytesToRead = serialPort.BytesToRead;
                            byte[] buffer = new byte[bytesToRead];
                            serialPort.Read(buffer, 0, bytesToRead);
                            response = ByteToHex(buffer);
                            break;
                        }
                }
                UpdateLogWindow("Response:" + response + "\n");
                SaveResponse("[ Request: " +CurrentCmd.Item4+ " ][ Response: " + response + " ]", CurrentCmd.Item3);
            }
            catch(Exception ex)
            {
                UpdateLogWindow("Receiver Error:"+ex.Message + "\n");

            }
        }

        private void SaveResponse(string response, bool IsManualCmd)
        {
            FormRef.SaveResponse(response);
        }

        private void UpdateLogWindow(string log, bool init = false)
        {
            try {
                if (init)
                    FormRef.rtbDisplay.Clear();
                FormRef.rtbDisplay.AppendText(log);
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
