namespace Denyo.ConnectionBridge.Client
{
    partial class NotificationClient
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotificationClient));
            this.CBNotifier = new System.Windows.Forms.NotifyIcon(this.components);
            this.cMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dgView = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.rtbDisplay = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).BeginInit();
            this.SuspendLayout();
            // 
            // CBNotifier
            // 
            this.CBNotifier.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.CBNotifier.Icon = ((System.Drawing.Icon)(resources.GetObject("CBNotifier.Icon")));
            this.CBNotifier.Text = "Connection Bridge (Active)";
            this.CBNotifier.Visible = true;
            this.CBNotifier.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CBNotifier_MouseDoubleClick);
            // 
            // cMenu
            // 
            this.cMenu.Name = "cMenu";
            this.cMenu.Size = new System.Drawing.Size(61, 4);
            this.cMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cMenu_Opening);
            // 
            // dgView
            // 
            this.dgView.AllowUserToDeleteRows = false;
            this.dgView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgView.Location = new System.Drawing.Point(12, 40);
            this.dgView.Name = "dgView";
            this.dgView.ReadOnly = true;
            this.dgView.Size = new System.Drawing.Size(375, 354);
            this.dgView.TabIndex = 1;
            this.dgView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgView_CellContentClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection Bridge Client Status";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(311, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "&Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(394, 8);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Start Worker";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // rtbDisplay
            // 
            this.rtbDisplay.Location = new System.Drawing.Point(394, 40);
            this.rtbDisplay.Name = "rtbDisplay";
            this.rtbDisplay.Size = new System.Drawing.Size(346, 354);
            this.rtbDisplay.TabIndex = 5;
            this.rtbDisplay.Text = "";
            // 
            // NotificationClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 406);
            this.Controls.Add(this.rtbDisplay);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationClient";
            this.Text = "ConnectionBridge Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NotificationClient_FormClosing);
            this.Load += new System.EventHandler(this.NotificationClient_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon CBNotifier;
        public System.Windows.Forms.ContextMenuStrip cMenu;
        private System.Windows.Forms.DataGridView dgView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox rtbDisplay;
    }
}