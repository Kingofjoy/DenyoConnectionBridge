using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace GPSTest
{
    class Program
    {
        static SerialPort commGPSSerialPort;
        static void Main(string[] args)
        {
            try
            {

                string resp=@" AT$GPSACP $GPSACP: 104335.003,0118.6000N,10351.6000E,0.0,-5.0,0,0.0,0.0,0.0,150218,00

OK
 ";
                Console.WriteLine(resp);
                Console.WriteLine(resp.Substring(resp.IndexOf(":")));

                Console.WriteLine(resp.Substring(resp.IndexOf(":")+2, resp.IndexOf("OK")- resp.IndexOf(":")-5));


                Console.WriteLine("GPS TEST START");

                commGPSSerialPort = new SerialPort();


                if (commGPSSerialPort.IsOpen)
                {
                    commGPSSerialPort.Close();
                }

                Console.WriteLine("Setting props");

                commGPSSerialPort.BaudRate = 115200;
                commGPSSerialPort.StopBits = StopBits.One;
                commGPSSerialPort.DataBits = 8;
                commGPSSerialPort.Parity = Parity.None;
                commGPSSerialPort.PortName = "COM6";
                commGPSSerialPort.DtrEnable = true;
                commGPSSerialPort.RtsEnable = true;

                commGPSSerialPort.DataReceived += CommGPSSerialPort_DataReceived;

                Console.WriteLine("Props set");

                commGPSSerialPort.Open();

                Console.WriteLine("Oprt Opened");

                WriteToSerial("41 54 45 31 0d 0d 0a 4f 4b 0d 0a");

                Console.ReadLine();

                WriteToSerial("41 54 24 47 50 53 41 54 3d 31 0d");

                Console.ReadLine();

                WriteToSerial("41 54 24 47 50 53 41 43 50 0d");

                
                Console.ReadLine();

                
            }
            catch(Exception Ex)
            {
                Console.Write("ERROR ENC " + Ex.Message);
                Console.ReadLine();
            }
        }

        private static void WriteToSerial(string CommandToSend)
        {
            try
            {
                Console.WriteLine("Sending "+ CommandToSend+ " to serial");
                byte[] buffer = HexToByte(CommandToSend);
                commGPSSerialPort.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Sending completed");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while writing serial data..." + ex.Message);
            }
        }

        private static byte[] HexToByte(string cmd)
        {
            cmd = cmd.Replace(" ", "");
            byte[] buffer = new byte[cmd.Length / 2];
            for (int i = 0; i < cmd.Length; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(cmd.Substring(i, 2), 0x10);
            }
            return buffer;
        }

        private static void CommGPSSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Console.WriteLine("CommGPSSerialPort_DataReceived");

                int bytesToRead = commGPSSerialPort.BytesToRead;

                Console.WriteLine(bytesToRead);

                string Response = string.Empty;

                byte[] buffer2 = new byte[bytesToRead];
                commGPSSerialPort.Read(buffer2, 0, bytesToRead);

                Console.WriteLine(buffer2);

                Response += ByteToHex(buffer2);

                Console.WriteLine(Response);

                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while receiving serial data..."+ ex.Message);
            }
        }

        private static string ByteToHex(byte[] comByte)
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
