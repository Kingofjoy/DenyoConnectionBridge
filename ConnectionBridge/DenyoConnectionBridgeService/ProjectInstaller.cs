using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;


namespace DenyoConnectionBridgeService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            //RegisterInStartup(true);
        }

        
        //private void RegisterInStartup(bool isChecked)
        //{
        //    try
        //    {

        //        //var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        //        //if (registryKey.GetValue("Connection_Bridge_Client_Startup") == null || (string)registryKey.GetValue("Connection_Bridge_Client_Startup") != Assembly.GetExecutingAssembly().Location)
        //        //{
        //        //    registryKey.SetValue("Connection_Bridge_Client_Startup", Assembly.GetExecutingAssembly().Location);
        //        //}

        //        string t_registeryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        //        RegistryKey registryKey =
        //            Registry.LocalMachine.OpenSubKey(t_registeryPath, true);

        //        if (registryKey == null)
        //            registryKey = Registry.LocalMachine.CreateSubKey(t_registeryPath);

        //        if (isChecked)
        //        {
        //            string tgt_dir = Context.Parameters["targetPath"];
        //            if (!Directory.Exists(tgt_dir))
        //                return;

        //            string t_exeName = Path.Combine(tgt_dir, "ConnectionBridgeClient.exe");
        //            if (!File.Exists(t_exeName))
        //                return;

        //            registryKey.SetValue("Connection_Bridge_Client_Startup", t_exeName);
        //        }
        //        else
        //        {
        //            registryKey.DeleteValue("Connection_Bridge_Client_Startup");
        //        }
        //    }
        //    catch
        //    {
        //        return;
        //    }
        //}

        
    }
}
