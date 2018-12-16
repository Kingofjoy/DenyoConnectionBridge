using Denyo.ConnectionBridge;
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
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            Console.WriteLine("START");

            Main_noUI objProcessor = new Main_noUI();
            Logger.ThreadLife = true;
            objProcessor.Process();
            Console.WriteLine("objProcessor start complete");

            //WebAPIHandler apiHandler = new WebAPIHandler();

            //apiHandler.Start();

            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
            Console.WriteLine("END");
            Console.ReadLine();
        }
    }
}
