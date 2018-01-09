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


        public SerialPortHandler(int BaudRate, int DataBits, StopBits StopBits, Parity Parity, string PortName)
        {
            serialPort = new SerialPort();
            _baudRate = BaudRate;
            _dataBits = DataBits;
            _stopBits = StopBits;
            _parity = Parity;
            _portName = PortName;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceiver);
            GotResponseForPrevCmd = false;
            _mode = CommunicationMode.HEXA;
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

        public void SendNextCommand(string cmd, CommunicationMode mode)
        {
            _mode = mode;
            if (!GotResponseForPrevCmd)
            {
                return;
            }
            else
            {
                DataSender(cmd);
            }
        }

        public void SendManualCommand(string cmd)
        {
            _mode = CommunicationMode.TEXT;
            DataSender(cmd);
        }

        private void DataSender(string cmd)
        {
            switch (_mode)
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
            GotResponseForPrevCmd = true;
            Main.cmdCounter++;

            switch (_mode)
            {
                case CommunicationMode.TEXT:
                    UpdateLogWindow(serialPort.ReadExisting() + "\n");
                    return;

                case CommunicationMode.HEXA:
                    {
                        int bytesToRead = serialPort.BytesToRead;
                        byte[] buffer = new byte[bytesToRead];
                        serialPort.Read(buffer, 0, bytesToRead);
                        UpdateLogWindow(ByteToHex(buffer) + "\n");
                        return;
                    }
            }
        }

        private void UpdateLogWindow(string log)
        {
            FormRef.rtbDisplay.AppendText(log);
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
