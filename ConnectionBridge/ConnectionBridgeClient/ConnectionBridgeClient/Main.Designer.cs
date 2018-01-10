using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace Denyo.ConnectionBridge.Client
{
    partial class Main
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
            this.label5 = new System.Windows.Forms.Label();
            this.cboData = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboStop = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboParity = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.cboBaud = new System.Windows.Forms.ComboBox();
            this.cboPort = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cmdSend = new System.Windows.Forms.Button();
            this.txtSend = new System.Windows.Forms.TextBox();
            this.rtbDisplay = new System.Windows.Forms.RichTextBox();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoText = new System.Windows.Forms.RadioButton();
            this.rdoHex = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lblHC = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();
            this.lblRemoteServer = new System.Windows.Forms.Label();
            this.lblInternet = new System.Windows.Forms.Label();
            this.lblDevice = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.GroupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 220);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 17);
            this.label5.TabIndex = 19;
            this.label5.Text = "Data Bits";
            // 
            // cboData
            // 
            this.cboData.FormattingEnabled = true;
            this.cboData.Items.AddRange(new object[] {
            "7",
            "8",
            "9"});
            this.cboData.Location = new System.Drawing.Point(12, 240);
            this.cboData.Margin = new System.Windows.Forms.Padding(4);
            this.cboData.Name = "cboData";
            this.cboData.Size = new System.Drawing.Size(100, 24);
            this.cboData.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 171);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 17);
            this.label4.TabIndex = 18;
            this.label4.Text = "Stop Bits";
            // 
            // cboStop
            // 
            this.cboStop.FormattingEnabled = true;
            this.cboStop.Location = new System.Drawing.Point(12, 191);
            this.cboStop.Margin = new System.Windows.Forms.Padding(4);
            this.cboStop.Name = "cboStop";
            this.cboStop.Size = new System.Drawing.Size(100, 24);
            this.cboStop.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 121);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 17);
            this.label3.TabIndex = 17;
            this.label3.Text = "Parity";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "Baud Rate";
            // 
            // cboParity
            // 
            this.cboParity.FormattingEnabled = true;
            this.cboParity.Location = new System.Drawing.Point(12, 140);
            this.cboParity.Margin = new System.Windows.Forms.Padding(4);
            this.cboParity.Name = "cboParity";
            this.cboParity.Size = new System.Drawing.Size(100, 24);
            this.cboParity.TabIndex = 12;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(8, 22);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(34, 17);
            this.Label1.TabIndex = 15;
            this.Label1.Text = "Port";
            // 
            // cboBaud
            // 
            this.cboBaud.FormattingEnabled = true;
            this.cboBaud.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "14400",
            "28800",
            "36000",
            "115000"});
            this.cboBaud.Location = new System.Drawing.Point(12, 91);
            this.cboBaud.Margin = new System.Windows.Forms.Padding(4);
            this.cboBaud.Name = "cboBaud";
            this.cboBaud.Size = new System.Drawing.Size(100, 24);
            this.cboBaud.TabIndex = 11;
            // 
            // cboPort
            // 
            this.cboPort.FormattingEnabled = true;
            this.cboPort.Location = new System.Drawing.Point(12, 42);
            this.cboPort.Margin = new System.Windows.Forms.Padding(4);
            this.cboPort.Name = "cboPort";
            this.cboPort.Size = new System.Drawing.Size(100, 24);
            this.cboPort.TabIndex = 10;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(578, 318);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(140, 28);
            this.button1.TabIndex = 14;
            this.button1.Text = "Get MetaData";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cmdSend
            // 
            this.cmdSend.Location = new System.Drawing.Point(435, 316);
            this.cmdSend.Margin = new System.Windows.Forms.Padding(4);
            this.cmdSend.Name = "cmdSend";
            this.cmdSend.Size = new System.Drawing.Size(100, 28);
            this.cmdSend.TabIndex = 5;
            this.cmdSend.Text = "Send";
            this.cmdSend.UseVisualStyleBackColor = true;
            this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click_1);
            // 
            // txtSend
            // 
            this.txtSend.Location = new System.Drawing.Point(9, 319);
            this.txtSend.Margin = new System.Windows.Forms.Padding(4);
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(400, 22);
            this.txtSend.TabIndex = 4;
            // 
            // rtbDisplay
            // 
            this.rtbDisplay.Location = new System.Drawing.Point(9, 23);
            this.rtbDisplay.Margin = new System.Windows.Forms.Padding(4);
            this.rtbDisplay.Name = "rtbDisplay";
            this.rtbDisplay.ReadOnly = true;
            this.rtbDisplay.Size = new System.Drawing.Size(723, 287);
            this.rtbDisplay.TabIndex = 3;
            this.rtbDisplay.Text = "";
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.button1);
            this.GroupBox1.Controls.Add(this.cmdSend);
            this.GroupBox1.Controls.Add(this.txtSend);
            this.GroupBox1.Controls.Add(this.rtbDisplay);
            this.GroupBox1.Location = new System.Drawing.Point(23, 121);
            this.GroupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.GroupBox1.Size = new System.Drawing.Size(741, 354);
            this.GroupBox1.TabIndex = 11;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Log";
            // 
            // rdoText
            // 
            this.rdoText.AutoSize = true;
            this.rdoText.Location = new System.Drawing.Point(8, 47);
            this.rdoText.Margin = new System.Windows.Forms.Padding(4);
            this.rdoText.Name = "rdoText";
            this.rdoText.Size = new System.Drawing.Size(56, 21);
            this.rdoText.TabIndex = 1;
            this.rdoText.TabStop = true;
            this.rdoText.Text = "Text";
            this.rdoText.UseVisualStyleBackColor = true;
            // 
            // rdoHex
            // 
            this.rdoHex.AutoSize = true;
            this.rdoHex.Location = new System.Drawing.Point(8, 20);
            this.rdoHex.Margin = new System.Windows.Forms.Padding(4);
            this.rdoHex.Name = "rdoHex";
            this.rdoHex.Size = new System.Drawing.Size(53, 21);
            this.rdoHex.TabIndex = 0;
            this.rdoHex.TabStop = true;
            this.rdoHex.Text = "Hex";
            this.rdoHex.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rdoText);
            this.groupBox3.Controls.Add(this.rdoHex);
            this.groupBox3.Location = new System.Drawing.Point(780, 403);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(133, 74);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mode";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.cboData);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.cboStop);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cboParity);
            this.groupBox2.Controls.Add(this.Label1);
            this.groupBox2.Controls.Add(this.cboBaud);
            this.groupBox2.Controls.Add(this.cboPort);
            this.groupBox2.Location = new System.Drawing.Point(783, 124);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(128, 272);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Location = new System.Drawing.Point(22, 13);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(732, 100);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Status";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(290, 66);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 17);
            this.label9.TabIndex = 3;
            this.label9.Text = "Timer : ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(231, 34);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(115, 17);
            this.label8.TabIndex = 2;
            this.label8.Text = "Remote Server : ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 66);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "Internet : ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Device : ";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lblHC);
            this.groupBox5.Controls.Add(this.lblTime);
            this.groupBox5.Controls.Add(this.lblTimer);
            this.groupBox5.Controls.Add(this.lblRemoteServer);
            this.groupBox5.Controls.Add(this.lblInternet);
            this.groupBox5.Controls.Add(this.lblDevice);
            this.groupBox5.Controls.Add(this.label14);
            this.groupBox5.Controls.Add(this.label15);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Location = new System.Drawing.Point(22, 13);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(732, 100);
            this.groupBox5.TabIndex = 14;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Status";
            // 
            // lblHC
            // 
            this.lblHC.AutoSize = true;
            this.lblHC.Location = new System.Drawing.Point(580, 66);
            this.lblHC.Name = "lblHC";
            this.lblHC.Size = new System.Drawing.Size(27, 17);
            this.lblHC.TabIndex = 11;
            this.lblHC.Text = "NA";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(580, 34);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(27, 17);
            this.lblTime.TabIndex = 10;
            this.lblTime.Text = "NA";
            // 
            // lblTimer
            // 
            this.lblTimer.AutoSize = true;
            this.lblTimer.Location = new System.Drawing.Point(352, 66);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(27, 17);
            this.lblTimer.TabIndex = 9;
            this.lblTimer.Text = "NA";
            // 
            // lblRemoteServer
            // 
            this.lblRemoteServer.AutoSize = true;
            this.lblRemoteServer.Location = new System.Drawing.Point(352, 34);
            this.lblRemoteServer.Name = "lblRemoteServer";
            this.lblRemoteServer.Size = new System.Drawing.Size(27, 17);
            this.lblRemoteServer.TabIndex = 8;
            this.lblRemoteServer.Text = "NA";
            // 
            // lblInternet
            // 
            this.lblInternet.AutoSize = true;
            this.lblInternet.Location = new System.Drawing.Point(89, 66);
            this.lblInternet.Name = "lblInternet";
            this.lblInternet.Size = new System.Drawing.Size(27, 17);
            this.lblInternet.TabIndex = 7;
            this.lblInternet.Text = "NA";
            // 
            // lblDevice
            // 
            this.lblDevice.AutoSize = true;
            this.lblDevice.Location = new System.Drawing.Point(89, 34);
            this.lblDevice.Name = "lblDevice";
            this.lblDevice.Size = new System.Drawing.Size(27, 17);
            this.lblDevice.TabIndex = 6;
            this.lblDevice.Text = "NA";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(535, 66);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(39, 17);
            this.label14.TabIndex = 5;
            this.label14.Text = "HC : ";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(523, 34);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 17);
            this.label15.TabIndex = 4;
            this.label15.Text = "Time : ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(290, 66);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(56, 17);
            this.label10.TabIndex = 3;
            this.label10.Text = "Timer : ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(231, 34);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(115, 17);
            this.label11.TabIndex = 2;
            this.label11.Text = "Remote Server : ";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 66);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 17);
            this.label12.TabIndex = 1;
            this.label12.Text = "Internet : ";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(20, 34);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(63, 17);
            this.label13.TabIndex = 0;
            this.label13.Text = "Device : ";
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(926, 488);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Name = "Main";
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboData;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboParity;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.ComboBox cboBaud;
        private System.Windows.Forms.ComboBox cboPort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button cmdSend;
        private System.Windows.Forms.TextBox txtSend;
        public System.Windows.Forms.RichTextBox rtbDisplay;
        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.RadioButton rdoText;
        private System.Windows.Forms.RadioButton rdoHex;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Label lblRemoteServer;
        private System.Windows.Forms.Label lblInternet;
        private System.Windows.Forms.Label lblDevice;
        private System.Windows.Forms.Label lblHC;
        private System.Windows.Forms.Label label14;
        public System.Windows.Forms.Timer timer1;
    }
}