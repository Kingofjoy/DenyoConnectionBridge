using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.Server.TCPServer;
using System.ComponentModel;

using Denyo.ConnectionBridge.Server.WebServer;
using System.Collections.Concurrent;
using Denyo.ConnectionBridge.MySqlDBConnection;
using Denyo.ConnectionBridge.DataStructures;

namespace TestHost
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Denyo.ConnectionBridge.Server.TCPServer.Server tcpHost = null;
        static Denyo.ConnectionBridge.Server.WebServer.Server webHost = null;
        
        static void Main(string[] args)
        {
            //Console.WriteLine("Test");
            //DatabaseManager dbInteraction = new DatabaseManager();
            //dbInteraction.UpdateSetPoints("GEN0002", "VOLT", "EP", "70", "70", DateTime.Now, "APT");

            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 00 B8 44", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 02 11 79 28", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 06 31 7A 30", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 06 36 3B F2", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 02 11 79 28", "ENGSPEED"));

            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 01 55 79 EB", "COLLANTTEMP"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 01 54 B8 2B", "COLLANTTEMP"));

            //Console.WriteLine(System.Text.RegularExpressions.Regex.IsMatch(" Wrn Low Load ! ", @"^[a-zA-Z0-9 ]+$"));

            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 36 00 00 01 00 2A 57 72 6E 20 4C 6F 77 20 4C 6F 61 64 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E9 DB", "A"));

            //Console.WriteLine(System.Text.RegularExpressions.Regex.IsMatch(Converter.HexaToString("01 03 36 00 00 01 00 2A 57 72 6E 20 4C 6F 77 20 4C 6F 61 64 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E9 DB", "A"), @"^[a-zA-Z0-9 ]+$")); 

            //Console.WriteLine("Test");
            //Console.WriteLine("Test");
            //Console.WriteLine("01 03 02 00 00 B8 44 , LOADPOWER1");
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 00 B8 44", "MODE"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 07 F9 86", "MODE"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 01 F7 F8 52", "FREQ"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 28 B8 5A", "LOADPOWER"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 64 B9 AF", "LOADPOWERFACTOR"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 01 79 84 01 03 02 05 E7 FB SE", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 8B F8 23 01 03 02 00 18 B8 4E", "OILPRESSURE"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 00 02 4B bb 64", "RUNNINGHR"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 06 2C BA 39", "ENGSPEED"));
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 36 00 00 02 00 2A 53 64 20 44 6F 6F 72 20 53 77 69 74 63 68 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3B F3", "A"));

            //01 03 02 00 6E
            //Console.ReadLine();
            Console.WriteLine("Init");

            ConcurrentDictionary<string, ConcurrentQueue<DataPacket>> MessageQueues = new ConcurrentDictionary<string, ConcurrentQueue<DataPacket>>();
            MessageQueues.AddOrUpdate("ReceivedMessages", new ConcurrentQueue<DataPacket>(),(k,x)=> { return new ConcurrentQueue<DataPacket>(); });
            MessageQueues.AddOrUpdate("PostMessages", new ConcurrentQueue<DataPacket>(), (k, x) => { return new ConcurrentQueue<DataPacket>(); });
            MessageQueues.AddOrUpdate("ProcessedMessages", new ConcurrentQueue<DataPacket>(), (k, x) => { return new ConcurrentQueue<DataPacket>(); });

            ConcurrentQueue<DataPacket> ReceivedMessages = new ConcurrentQueue<DataPacket>();
            ConcurrentQueue<DataPacket> PostMessages = new ConcurrentQueue<DataPacket>();

             
            //webHost = new DenyoCBWebAPI();

            tcpHost = new Denyo.ConnectionBridge.Server.TCPServer.Server(ref MessageQueues);
            tcpHost.Start();

            Console.WriteLine("TCP Server Started");

            webHost = new Denyo.ConnectionBridge.Server.WebServer.Server(ref MessageQueues);
            webHost.Start();

            Console.WriteLine("Web Server Started");

            //DataPacket deviceResponse = new DataPacket();

            //deviceResponse.SenderID = "GEN0002";
            //deviceResponse.SenderType = AppType.Client;

            //deviceResponse.RecepientID = "TSA0001";
            //deviceResponse.RecepientType = AppType.Server;

            //deviceResponse.Type = PacketType.MonitoringData;

            //deviceResponse.Message = "ALARMS,01 03 02 00 01 79 84";
            //deviceResponse.TimeStamp = DateTime.Now;

            //MessageQueues["ReceivedMessages"].Enqueue(deviceResponse);

            //deviceResponse = new DataPacket();

            //deviceResponse.SenderID = "GEN0002";
            //deviceResponse.SenderType = AppType.Client;

            //deviceResponse.RecepientID = "TSA0001";
            //deviceResponse.RecepientType = AppType.Server;

            //deviceResponse.Type = PacketType.MonitoringData;
            //deviceResponse.Message = "A,01 03 36 00 00 02 00 2A 53 64 20 44 6F 6F 72 20 53 77 69 74 63 68 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3B F3";
            //deviceResponse.TimeStamp = DateTime.Now;
            //MessageQueues["ReceivedMessages"].Enqueue(deviceResponse);

            //deviceResponse = new DataPacket();

            //deviceResponse.SenderID = "GEN0002";
            //deviceResponse.SenderType = AppType.Client;

            //deviceResponse.RecepientID = "TSA0001";
            //deviceResponse.RecepientType = AppType.Server;

            //deviceResponse.Type = PacketType.MonitoringData;
            //deviceResponse.Message = "A,53 64 20 44 6F 6F 72 20 53 77 69 74 63 68 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 3B F3";
            //deviceResponse.TimeStamp = DateTime.Now;
            //MessageQueues["ReceivedMessages"].Enqueue(deviceResponse);




            //work();


            //BackgroundWorker bwServerAcceptance = new BackgroundWorker();
            //BackgroundWorker bwReceiveData = new BackgroundWorker();

            //bwServerAcceptance.WorkerSupportsCancellation = true;
            //bwServerAcceptance.DoWork += BwServerAcceptance_DoWork;
            ////bwServerAcceptance.RunWorkerAsync();

            //Console.WriteLine("Accept Hander Initiated");


            //bwReceiveData.WorkerSupportsCancellation = true;
            //bwReceiveData.DoWork += BwReceiveData_DoWork; ;
            ////bwReceiveData.RunWorkerAsync();

            //Console.WriteLine("Receive Hander Initiated");

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "CLOSE")
                    break;
                else if (input == "CLEAR")
                {
                    Console.Clear();
                    continue;
                }
                else
                    continue;
            }


            Console.WriteLine("End");

            
        }

        static async void work()
        {
            try
            {


                //Task AcceptHandler = new Task(tcpHost.AcceptIncomingConnetion);
                //AcceptHandler.Start();
                //AcceptHandler.Wait();

                //Console.WriteLine("Accept Hander Initiated");

                //Task Messagewatcher= new Task(tcpHost.ReceiveMessages);
                //Messagewatcher.Start();
                //Messagewatcher.Wait();


                //TaskFactory TList = new TaskFactory();
                //await TList.StartNew(tcpHost.AcceptIncomingConnetion);
                //await TList.StartNew(tcpHost.ReceiveMessages);


                //Console.WriteLine("Receive Hander Initiated");


                //Console.WriteLine("End");
            }
            catch (Exception ex)
            {

            }
        }

        private static void BwReceiveData_DoWork(object sender, DoWorkEventArgs e)
        {
            tcpHost.ReceiveMessages();
        }

        private static void BwServerAcceptance_DoWork(object sender, DoWorkEventArgs e)
        {
            tcpHost.AcceptIncoming_Connection();
        }
    }
}
