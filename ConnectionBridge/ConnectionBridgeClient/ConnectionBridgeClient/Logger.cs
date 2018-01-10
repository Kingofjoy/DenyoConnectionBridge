using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Client
{
    public static class Logger
    {
        private static bool IsEnabled;
        static Logger()
        {
            // log file
            IsEnabled = bool.Parse(ConfigurationManager.AppSettings["Logger"]);
        }

        public static void Log(string msg)
        {
            if(IsEnabled)
            {

            }
        }

        public static void Log(string msg, Exception ex)
        {
            if (IsEnabled)
            {

            }
        }
    }
}
