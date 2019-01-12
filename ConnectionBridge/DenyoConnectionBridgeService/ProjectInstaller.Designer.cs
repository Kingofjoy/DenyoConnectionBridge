namespace DenyoConnectionBridgeService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DelayedAutoStart = true;
            this.serviceInstaller1.Description = "DenyoConnectionBridgeService";
            this.serviceInstaller1.DisplayName = "Denyo ConnectionBridge Service";
            this.serviceInstaller1.ServiceName = "DenyoConnectionBridgeService";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            //this.serviceInstaller1.Committed += ServiceInstaller1_Committed;
            
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1});

        }

        //private void ServiceInstaller1_Committed(object sender, System.Configuration.Install.InstallEventArgs e)
        //{
        //    string tardir = "";
        //    try
        //    {
        //        tardir = Context.Parameters["[TARGETDIR]"].ToString();
        //    }
        //    catch { }
        //    try
        //    {
        //        tardir += " , " + Context.Parameters["TARGETDIR"].ToString();
        //    }
        //    catch { }

        //    try
        //    {
        //        System.Diagnostics.Process.Start("postinstaller.bat", tardir);
        //    }
        //    catch
        //    { }
        //}

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}