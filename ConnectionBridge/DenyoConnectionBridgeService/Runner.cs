using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DenyoConnectionBridgeService
{
    class Runner
    {
        string fileName = @"E:\\Temp\\Sample2.txt";
        object mylock=new object();

        double timerdelay = 2000;
        System.Timers.Timer srvTimer2;
        public Runner()
        {
            timerdelay = 1100;
            srvTimer2 =  new System.Timers.Timer(timerdelay);
            
            srvTimer2.Elapsed += new System.Timers.ElapsedEventHandler(srvTimer2_Elapsed);
            srvTimer2.Enabled = true;
        }

        public void start()
        {
            timerdelay = 1000;
            srvTimer2 = new System.Timers.Timer(timerdelay);
            
            srvTimer2.Elapsed += new System.Timers.ElapsedEventHandler(srvTimer2_Elapsed);
            srvTimer2.Enabled = true;
        }

        void srvTimer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                lock (mylock)
                {
                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        sw.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "RUNNER" + System.Threading.Thread.CurrentThread.ManagedThreadId);

                    }
                }
            }
            catch(Exception ex) { }
        }


        public void manualRun()
        {
            try
            {
                while (true)
                {
                    lock (mylock)
                    {
                        using (StreamWriter sw = File.AppendText(fileName))
                        {
                            sw.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "MNRUNNER" + System.Threading.Thread.CurrentThread.ManagedThreadId);

                        }
                    }
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch { }
        }



    }
}
