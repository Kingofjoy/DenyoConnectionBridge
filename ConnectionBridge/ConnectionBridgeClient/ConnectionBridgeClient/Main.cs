using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;

using Denyo.ConnectionBridge.DataStructures;

namespace Denyo.ConnectionBridge.Client
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            InitializeMetaData();

            InitializeFormSerialParams();
        }

        private void InitializeFormSerialParams()
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        private void InitializeMetaData()
        {
            try
            {
                Metadata.AppID = ConfigurationManager.AppSettings["AppID"];
                Metadata.AppType = ConfigurationManager.AppSettings["AppType"];
                Metadata.AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                Metadata.PreferredCOMPort = ConfigurationManager.AppSettings["LCPort"];
                Metadata.PreferredBaudRate = ConfigurationManager.AppSettings["LCPBaud"];

                Metadata.ServerIP = ConfigurationManager.AppSettings["RServer"];
                Metadata.ServerPort= int.Parse(ConfigurationManager.AppSettings["RSPort"]);

            }
            catch(Exception imEx1)
            {

            }

            try
            {
                string HexaConfigFile = ConfigurationManager.AppSettings["HexaDictionary"];
                if(string.IsNullOrEmpty(HexaConfigFile))
                {
                    MessageBox.Show("Unable to find HexaConfig");
                    Application.Exit();
                    //throw new Exception("Unable to find HexaConfig");
                }

                if(!System.IO.File.Exists(HexaConfigFile))
                {
                    MessageBox.Show("Unable to find Hexa Config File");
                    Application.Exit();
                }

                foreach(string strlineitem in System.IO.File.ReadLines(HexaConfigFile))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(strlineitem) || string.IsNullOrWhiteSpace(strlineitem))
                            continue;
                        HexaInput hIN = new HexaInput();
                        hIN.Raw = strlineitem;
                        hIN.Hexa = strlineitem.Split(",".ToCharArray())[0];
                        hIN.Name = strlineitem.Split(",".ToCharArray())[1];
                        hIN.PX = strlineitem.Substring(0, strlineitem.Length - strlineitem.IndexOf(','));
                    }
                    catch{ }
                }
            }
            catch(Exception imEx2)
            {

            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
