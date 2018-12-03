using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Denyo.ConnectionBridge.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            if (ConfigurationManager.AppSettings["UI_ENABLED"] != null && ConfigurationManager.AppSettings["UI_ENABLED"].ToString().ToLower() == "true")
            {
                DateTime lastKnownExceptionTime = DateTime.Now;


                int ErrorRestarts = 0;
                bool bFlag = false;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                while (ErrorRestarts <= 5 && !bFlag)
                {
                    try
                    {
                        Logger.LogFatal("GLOBAL APP INIT");

                        bFlag = true;
                        Application.Run(new Main());

                        Logger.LogFatal("GLOBAL APP EXIT");
                    }
                    catch (Exception glbExcept)
                    {
                        bFlag = false;
                        ErrorRestarts++;

                        Logger.LogFatal("GLOBAL EXCEPTION", glbExcept);

                        if (glbExcept.InnerException != null)
                        {
                            Logger.LogFatal("GLOBAL EXCEPTION-i", glbExcept.InnerException);
                        }

                        Logger.LogFatal("GLOBAL EXC" + ErrorRestarts);

                        System.Threading.Thread.Sleep(5000);

                        if ((DateTime.Now - lastKnownExceptionTime).Hours >= 1)
                        {
                            Logger.LogFatal("GLOBAL ERRST" + ErrorRestarts);
                            ErrorRestarts = 0;
                        }

                        lastKnownExceptionTime = DateTime.Now;

                        if (ErrorRestarts > 5) throw glbExcept;

                        try
                        {
                            GC.Collect();
                        }
                        catch { }
                    }
                }

            }
            else
            {
                Application.Run(new NotificationClient());
            }
        }

        
    }
}
