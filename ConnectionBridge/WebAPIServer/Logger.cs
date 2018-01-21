using Denyo.ConnectionBridge.DataStructures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIServer
{
    public static class Logger
    {
        private static bool IsEnabled;

        private static LogType Type;

        static Logger()
        {
            // log file
            IsEnabled = bool.Parse(ConfigurationManager.AppSettings["Logger"]);
            Type = LogType.Message;
        }

        public static void Log(string msg)
        {
            try
            {
                
            }
            catch (Exception ex)
            {

            }

            if (IsEnabled)
            {

            }
        }

        public static void Log(string msg, Exception ex)
        {
            if (IsEnabled)
            {
                // log type shold be error
            }
        }

    }
}
