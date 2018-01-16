using Denyo.ConnectionBridge.DataStructures;
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

        private static LogType Type;

        public static Main FormRef { get; set; }

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
                if (!(FormRef == null))
                {
                    FormRef.rtbDisplay.AppendText(msg);
                    FormRef.rtbDisplay.AppendText(Environment.NewLine);
                    if(FormRef.rtbDisplay.TextLength>5000)
                    {
                        FormRef.rtbDisplay.Text = FormRef.rtbDisplay.Text.Substring(FormRef.rtbDisplay.Text.IndexOf(Environment.NewLine, 0, 10));
                    }
                }
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
