using Denyo.ConnectionBridge.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            Console.WriteLine("START");

            Main_noUI objProcessor = new Main_noUI();

            objProcessor.Process();

            //WebAPIHandler apiHandler = new WebAPIHandler();
           
            //apiHandler.Start();

            while (true) { }


            Console.WriteLine("END");
            Console.ReadLine();
        }
    }
}
