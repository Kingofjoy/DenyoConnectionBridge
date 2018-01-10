using Denyo.ConnectionBridge.DataStructures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Client
{
    public class TcpClientHandler
    {
        TcpClient Client { get; set; }

        NetworkStream ServerStream { get; set; }

        string CmdFromServer = string.Empty;

        BackgroundWorker bwReceiver = new BackgroundWorker();


        public string AuthToken { get; set; }

        public AppType Type { get; set; }

        public string AppID { get; set; }

        public string AppType { get; set; }

        private string RSName { get; set; }

        private int RSPort { get; set; }

        DataPacket cmd = new DataPacket();


        public TcpClientHandler()
        {
            try
            {
                AppID = ConfigurationManager.AppSettings["AppID"];
                AppType = ConfigurationManager.AppSettings["AppType"];
                AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                RSName = ConfigurationManager.AppSettings["RServer"];
                RSPort = int.Parse(ConfigurationManager.AppSettings["RSPort"]);

                Client.Connect(RSName, RSPort);
            }
            catch (Exception ex)
            {

            }
        }

        private void BwReceiver_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (Client.Connected)
                {
                    try
                    {
                        if (!ServerStream.DataAvailable) continue;

                        byte[] inStream = new byte[Client.ReceiveBufferSize];

                        int recb = ServerStream.Read(inStream, 0, Client.ReceiveBufferSize);
                        if (recb == 0) return;

                        string returndata = System.Text.Encoding.ASCII.GetString(inStream);

                        if (returndata.Length == 0)
                        {
                            return;
                        }

                        cmd = new DataPacket();
                        cmd = JsonConvert.DeserializeObject<DataPacket>(returndata);

                        if (cmd.SenderID != string.Empty)
                        {
                           
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error while receiving. " + ex.Message);
                    }
                }

                Logger.Log("Server connection lost!");
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
    }
}
