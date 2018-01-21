﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.Server.TCPServer;
using System.ComponentModel;

using Denyo.ConnectionBridge.Server.WebServer;
using System.Collections.Concurrent;
using Denyo.ConnectionBridge.DataStructures;

namespace TestHost
{
    class Program
    {
        static Denyo.ConnectionBridge.Server.TCPServer.Server tcpHost = null;
        static Denyo.ConnectionBridge.Server.WebServer.Server webHost = null;
        
        static void Main(string[] args)
        {
            //Console.WriteLine("Test");
            //Console.WriteLine("01 03 02 00 00 B8 44 , LOADPOWER1");
            //Console.WriteLine(Denyo.ConnectionBridge.DataStructures.Converter.HexaToString("01 03 02 00 00 B8 44", "LOADPOWER1"));

            //Console.ReadLine();
            Console.WriteLine("Init");

            ConcurrentQueue<DataPacket> ReceivedMessages = new ConcurrentQueue<DataPacket>();
            ConcurrentQueue<DataPacket> PostMessages = new ConcurrentQueue<DataPacket>();


            //webHost = new DenyoCBWebAPI();

            tcpHost = new Denyo.ConnectionBridge.Server.TCPServer.Server(ref ReceivedMessages, ref PostMessages);
            tcpHost.Start();
            

            webHost = new Denyo.ConnectionBridge.Server.WebServer.Server(ref ReceivedMessages, ref PostMessages);
            webHost.Start();


            Console.WriteLine("Server Started");

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
