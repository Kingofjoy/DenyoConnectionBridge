using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.DataStructures;
using Newtonsoft.Json;

namespace Denyo.ConnectionBridge.Client
{
    public class TcpClientHandler
    {
        TcpClient Client { get; set; }

        NetworkStream ServerStream { get; set; }

        string CmdFromServer = string.Empty;

        BackgroundWorker bwReceiver = new BackgroundWorker();

        public Main FormRef { get; set; }

        private string AuthToken { get; set; }

        private AppType Type { get; set; }

        private string AppID { get; set; }

        private string AppType { get; set; }

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

                bwReceiver.WorkerSupportsCancellation = true;
                bwReceiver.DoWork += BwReceiver_DoWork;
                bwReceiver.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Logger.Log("TCP Handler Initialization failed.", ex);
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
                            continue;
                        }

                        cmd = new DataPacket();
                        cmd = JsonConvert.DeserializeObject<DataPacket>(returndata);

                        if (cmd.SenderID != string.Empty)
                        {
                            if(Metadata.InputDictionary.Where(_ => _.Name.Equals(cmd.Message)).Count() == 0 )
                            {
                                // send invalid cmd response to server
                                continue;
                            }
                            FormRef.SendManualCommand(cmd.Message);
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

        public void SendResponseToServer(string response)
        {

        }
    }
}
