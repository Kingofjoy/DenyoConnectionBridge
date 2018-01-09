using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Net;

using Denyo.ConnectionBridge.DataStructures;

namespace Denyo.ConnectionBridge.Server.TCPServer
{

    public static class Server
    {
        static TcpListener serverSocket = null;

        static List<Client> myClients = new List<Client>();

        static TcpClient clientSocket = default(TcpClient);
        static NetworkStream networkStream = null;
        static DateTime dtNow, dtNext;
        static bool bHBInit = false;

        public static int ListenerPort { get; }
        public static bool IsHeartBeatActive = false;
        static int HeartBeatInterval;

        static public bool IsActive { get; set; }

        static public string AppID { get; set; }

        static public string AppType { get; set; }

        static public string AuthToken { get; set; }

        static Server()
        {
           try
            {
                IsActive = false;

                bool.TryParse(ConfigurationManager.AppSettings["IsHeartBeatActive"], out IsHeartBeatActive);

                if (IsHeartBeatActive)
                    HeartBeatInterval = int.Parse(ConfigurationManager.AppSettings["HeartBeatInterval"]);

                AppID = ConfigurationManager.AppSettings["AppID"];
                AppType = ConfigurationManager.AppSettings["AppType"];
                AuthToken = ConfigurationManager.AppSettings["AuthToken"];

                ListenerPort = int.Parse(ConfigurationManager.AppSettings["SLPort"]);
            }
            catch(Exception ex)
            {

            }
        }

        public static bool Start()
        {
            try
            {
                serverSocket = new TcpListener(IPAddress.Any, ListenerPort));
                serverSocket.Start();

                IsActive = true;
            }
            catch(Exception ex)
            {

            }
            return IsActive;
        }

        public static void AcceptIncomingConnetion()
        {
            while (serverSocket.Pending())
            {
                Client myClient = new Client();
                myClient.Instance = serverSocket.AcceptTcpClient();
                myClient.Stream = myClient.Instance.GetStream();
            }
        }

        static void Log(string Message)
        {

        }

        static void Log(Exception ex,string Message="")
        {

        }

        static void Log(DataPacket packet)
        { }

    }

    public class Config
    {
        public int ListenerPort { get; set; }


    }
}
