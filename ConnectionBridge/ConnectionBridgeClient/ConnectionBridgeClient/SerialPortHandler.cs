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
        private SerialPort serialPort;

        private int _baudRate { get; set; }

        private int _dataBits { get; set; }

        private StopBits _stopBits { get; set; }

        private Parity _parity { get; set; }

        private string _portName { get; set; }

        private bool GotResponseForPrevCmd { get; set; }

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
        }
        private void ConnectionOpener()
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
            catch(Exception ex)
            {

            }
        }

        private void SendNextCommand()
        {

            if (!GotResponseForPrevCmd)
            {
                return;
            }
            else
            {
                DataSender("");
            }
        }

        private void SendManualCommand(string cmd)
        {
            // call SerialPortDataSender to send cmd
        }

        private void DataSender(string cmd)
        {

        }

        private void DataReceiver(object sender, SerialDataReceivedEventArgs e)
        {

        }

    }
}
