using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.Server.TCPServer;
using System.ComponentModel;

namespace TestHost
{
    class Program
    {
        static Server tcpHost = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Init");

            tcpHost = new Server();
            tcpHost.Start();

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
                if (Console.ReadLine() == "CLOSE")
                    break;
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
