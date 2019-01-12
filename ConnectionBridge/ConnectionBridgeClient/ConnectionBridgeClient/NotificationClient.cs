using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Reflection;

namespace Denyo.ConnectionBridge.Client
{
    public partial class NotificationClient : Form
    {
        Dictionary<string, string> StatusDic = new Dictionary<string, string>();
        System.Threading.Thread MyThread;
        bool goHide = false;
        bool setup = false;
        public NotificationClient(string[] args)
        {
            InitializeComponent();
            setup = true;
        }

        public NotificationClient()
        {
            InitializeComponent();

            cMenu.Items.Add("&Show Status");
            cMenu.Items.Add("&Quit");

            cMenu.Items[0].Click += NotificationClient_Click;
            cMenu.Items[1].Click += QuitMenu_Click;

            CBNotifier.ContextMenuStrip = cMenu;
            CBNotifier.Click += CBNotifier_Click;

            dgView.Columns.Add("StatusName", "Status");
            dgView.Columns.Add("StatusValue", "Value");
            dgView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            try
            {
                dgView.Rows.Clear();
                StatusDic = GetDataFromService();
                foreach(string ke in StatusDic.Keys)
                {
                    dgView.Rows.Add(ke, StatusDic[ke]);
                }
                dgView.Refresh();
            }catch(Exception ex)
            { }

            try
            {

                Logger.EventLogged += Logger_EventLogged;
            }
            catch { }

            this.Hide();

            this.CBNotifier.Visible = true;
            this.WindowState = FormWindowState.Minimized;
            goHide = true;
        }

        private void QuitMenu_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void NotificationClient_Click(object sender, EventArgs e)
        {
            this.Show();

            this.CBNotifier.Visible = false;
            this.WindowState = FormWindowState.Normal;

            try
            {
                dgView.Rows.Clear();
                StatusDic = GetDataFromService();
                foreach (string ke in StatusDic.Keys)
                {
                    dgView.Rows.Add(ke, StatusDic[ke]);
                }
                dgView.Refresh();
            }
            catch (Exception ex)
            { }
        }

        private void CBNotifier_Click(object sender, EventArgs e)
        {
           
        }

        private void NotificationClient_Load(object sender, EventArgs e)
        {
            if(setup)
            {
                SetupClientApplications();
                Application.Exit();
            }

            CBNotifier.BalloonTipTitle = "Denyo Connection Bridge";
            CBNotifier.BalloonTipText = "Denyo Connection Bridge Client Status";
            CBNotifier.ShowBalloonTip(1000);

            CBNotifier.Text = "Denyo Connection Bridge Client Status.";

            if(goHide)
            {
                this.Hide();
                this.CBNotifier.Visible = true;
                this.WindowState = FormWindowState.Minimized;
                goHide = false;
            }
        }

        private void CBNotifier_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();

            this.CBNotifier.Visible = false;
            this.WindowState = FormWindowState.Normal;

            try
            {
                dgView.Rows.Clear();
                StatusDic = GetDataFromService();
                foreach (string ke in StatusDic.Keys)
                {
                    dgView.Rows.Add(ke, StatusDic[ke]);
                }
                dgView.Refresh();
            }
            catch (Exception ex)
            { }
        }

        private void cMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        public Dictionary<string,string> GetDataFromService()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    //httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

                    var response = httpClient.GetStringAsync(new Uri(ConfigurationManager.AppSettings["CBClientAPI"])).Result;

                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                }
            }
            catch(Exception ex)
            {
                //Dict
                //return "Error while returning data"
                Dictionary<string, string> newDic = new Dictionary<string, string>();
                newDic.Add("DATAFETCHSTATE", "0");
                newDic.Add("DATAFETCHCODE", ex.Message);

                Exception iEx = ex.InnerException;
                int i=1;
                while (iEx != null)
                {
                    newDic.Add("DATAFETCHi" + i.ToString()+"_CODE", iEx.Message);
                    
                        iEx = iEx.InnerException;
                        i++;
                    
                }
                    
                return newDic;
            }
        }

        private void NotificationClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                this.Hide();
                
                this.CBNotifier.Visible = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void dgView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                dgView.Rows.Clear();
                StatusDic = GetDataFromService();
                foreach (string ke in StatusDic.Keys)
                {
                    dgView.Rows.Add(ke, StatusDic[ke]);
                }
                dgView.Refresh();
            }
            catch
            { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.LogFatal("Service Starting");
            Logger.ThreadLife = true;
            MyThread = new System.Threading.Thread(() =>
            {

                (new Main_noUI()).Process();

            });
            MyThread.SetApartmentState(ApartmentState.STA);
            MyThread.Start();
            Logger.LogFatal("Service Started");
        }

        private void UpdateLogWindow(string log, Exception exObj = null, DateTime? ReceivedDT = null)
        {

            DateTime dtObj = DateTime.Now;


            try
            {
                if (ReceivedDT != null) dtObj = (DateTime)ReceivedDT;

                if (this.rtbDisplay.TextLength > 100000)
                {
                    //FormRef.rtbDisplay.AppendText(dtObj.ToString("HH:mm:ss:ffff  > ") + log + "{" + FormRef.rtbDisplay.TextLength + "}");
                    this.rtbDisplay.SelectAll();
                    this.rtbDisplay.Clear();
                    this.rtbDisplay.Text = dtObj.ToString("HH:mm:ss:ffff  > ") + "Clear Disp";
                }
            }
            catch (Exception ex)
            {

            }

            try
            {
                //if (init)
                //    this.rtbDisplay.Clear();

                this.rtbDisplay.AppendText(dtObj.ToString("HH:mm:ss:ffff  > ") + log);
                this.rtbDisplay.AppendText(Environment.NewLine);

                if (exObj != null)
                {
                    this.rtbDisplay.AppendText(dtObj.ToString("HH:mm:ss:ffff  > E > ") + exObj.Message);
                    this.rtbDisplay.AppendText(Environment.NewLine);
                    if (exObj.InnerException != null)
                    {
                        this.rtbDisplay.AppendText(dtObj.ToString("HH:mm:ss:ffff  > IE > ") + exObj.InnerException.Message);
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }

        private void Logger_EventLogged(object sender, LoggerEventArgs e)
        {
            UpdateLogWindow(e.Type + " : " + e.Message, e.ExceptionObject, e.TimeStamp);
        }

        private static void SetupClientApplications()
        {
            try
            {
                #region SetClientAppInStartup

                var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey.GetValue("Connection_Bridge_Client_Startup") == null || (string)registryKey.GetValue("Connection_Bridge_Client_Startup") != Assembly.GetExecutingAssembly().Location)
                {
                    registryKey.SetValue("Connection_Bridge_Client_Startup", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }

                #endregion

                #region SetServiceRecovery
                System.Diagnostics.Process.Start(System.IO.Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\postinstaller.bat");
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("WRN: Invalid Setup Result. Ex " + ex.Message,"Warning");
            }
        }
    }
}
