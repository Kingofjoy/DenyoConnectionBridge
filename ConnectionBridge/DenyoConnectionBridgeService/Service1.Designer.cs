namespace DenyoConnectionBridgeService
{
    partial class Service1
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
            this.components = new System.ComponentModel.Container();
            this.srvTimer1 = new System.Timers.Timer(1000);
            // srvTimer1
            // 
            this.srvTimer1.Interval = 1000;
            this.srvTimer1.Elapsed +=new System.Timers.ElapsedEventHandler(srvTimer1_Elapsed); 
          
            // 
            // Service1
            // 
            this.ServiceName = "DenyoConnectionBridgeService";

        }

      

        #endregion

        private System.Timers.Timer srvTimer1;
    }
}
