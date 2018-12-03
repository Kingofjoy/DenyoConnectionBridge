using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Denyo.ConnectionBridge;
using Denyo.ConnectionBridge.Client;
using System.Configuration;

namespace DenyoConnectionBridgeService
{
    public partial class Service1 : ServiceBase
    {
        
        public Thread MyThread { get; set; }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //System.Threading.Thread.Sleep(5000);

            
            try
            {
                Logger.LogFatal("Service Starting");
                Logger.ThreadLife = true;
                MyThread = new System.Threading.Thread(() =>
                {

                    //var objRunner = new Runner();
                    ////objRunner.start();
                    //objRunner.manualRun();

                    (new Main_noUI()).Process();

                });
                MyThread.Start();
                
                Logger.LogFatal("CHILD THREAD. " + MyThread.ThreadState.ToString());
                Logger.LogFatal("CHILD THREAD. NewTID" + MyThread.ManagedThreadId);
                Logger.LogFatal("Service Started");
            }
            catch(Exception ex1)
            {
                Logger.LogFatal("Service Start Error", ex1);

                if(ex1.InnerException != null)
                    Logger.LogFatal("Service Start Error  IE", ex1.InnerException);
            }
            srvTimer1.Interval = int.Parse(ConfigurationManager.AppSettings["SRV_TIMER_INTERVAL"].ToString());
            srvTimer1.Enabled = true;
        }

        protected override void OnStop()
        {
            srvTimer1.Enabled = false;
            try
            {
                Logger.LogFatal("Service Stopping");

                Logger.ThreadLife = false;
                Thread.Sleep(500);
                Logger.LogFatal("CHILD THREAD. " + MyThread.ThreadState.ToString());

                if (MyThread != null)
                {
                    try
                    {
                        MyThread.Abort();
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogFatal("Error while stopping thread", ex2);

                        if (ex2.InnerException != null)
                            Logger.LogFatal("Error while stopping thread IE", ex2.InnerException);
                    }
                 
                }
                Logger.LogFatal("Service Stopped");
            }
            catch (Exception ex1)
            {
                Logger.LogFatal("Service Start Error", ex1);

                if (ex1.InnerException != null)
                    Logger.LogFatal("Service Start Error  IE", ex1.InnerException);
            }
        }

        void srvTimer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            srvTimer1.Enabled = false;
            try
            {
                Logger.Log("Service CB Instance Check started");

                if(DateTime.Now>Logger.WorkerHeartBeat)
                {
                    if(((TimeSpan)(DateTime.Now-Logger.WorkerHeartBeat)).Minutes <= 3)
                    {
                        Logger.Log("Last known HeartBeat is within 3 minutes. Skipping Thread Evaluation");
                        return;
                    }
                }

                if (MyThread == null || MyThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    if(MyThread == null)
                        Logger.LogFatal("Child CB Instance empty, reinitiating");
                    if(MyThread !=null)
                    {
                        Logger.LogFatal("Child CB Instance stopped, making empty and reinititating");

                        Logger.ThreadLife = false;
                        Thread.Sleep(500);
                        Logger.LogFatal("CHILD THREAD. " + MyThread.ThreadState.ToString());
                        try
                        {
                            MyThread.Suspend();
                        }
                        catch { }
                        try
                        {
                            MyThread.Abort();
                        }
                        catch { }
                        try
                        {
                            MyThread = null;
                        }
                        catch { }
                    }

                    try
                    {
                        Logger.ThreadLife = true;                     
                        MyThread = new System.Threading.Thread(() =>
                        {

                            (new Main_noUI()).Process();

                        });
                        MyThread.Start();
                        Logger.LogFatal("Child CB Instance Initiated");
                        Logger.LogFatal("Child CB Instance NewTID" + MyThread.ManagedThreadId);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogFatal("Error while reinitiating CB Child", ex2);

                        if (ex2.InnerException != null)
                            Logger.LogFatal("Error while reinitiating CB Child IE", ex2.InnerException);
                    }

                }
                else
                {
                    Logger.Log("Service CB Instance Check Pass: CB1: " + MyThread.ThreadState.ToString());
                }


                Logger.Log("Service CB Instance Check Completed");
            }
            catch(Exception dummyEx)
            {
                Logger.LogFatal("Service CB Instance Check Error. " + dummyEx.Message, dummyEx);
            }
            finally
            {
                srvTimer1.Enabled = true;
            }
        }

        //void OnFailure(Exception e)
        //{
        //    using (StreamWriter sw = File.AppendText(fileName))
        //    {
        //        sw.WriteLine(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + "SERVICE EXCEPTION");

        //        //TaskFactory.StartNewTask(() => new Executor().Execute(workToDo, OnFailure));

        //    }
        //}
       
    }
}
