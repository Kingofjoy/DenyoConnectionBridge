﻿using Denyo.ConnectionBridge.DataStructures;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    if (FormRef.rtbDisplay.TextLength > 100000)
                    {
                        //FormRef.rtbDisplay.AppendText(DateTime.Now.ToString("HH:mm:ss:ffff  > ") + log + "{" + FormRef.rtbDisplay.TextLength + "}");
                        FormRef.rtbDisplay.SelectAll();
                        FormRef.rtbDisplay.Clear();
                        FormRef.rtbDisplay.Text = DateTime.Now.ToString("HH:mm:ss:ffff  > ") + "Clear Disp" + Environment.NewLine;
                    }

                    FormRef.rtbDisplay.AppendText(DateTime.Now.ToString("HH:mm:ss:ffff  > ") + msg);
                    FormRef.rtbDisplay.AppendText(Environment.NewLine);
                   
                    log.Debug(msg);
                }
            }
            catch (Exception ex)
            {

            }

          
        }

        public static void Log(string msg, Exception ex)
        {
            if (IsEnabled)
            {
                log.Error(msg, ex);
            }
        }

    }
}
