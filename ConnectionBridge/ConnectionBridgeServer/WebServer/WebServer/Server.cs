using Denyo.ConnectionBridge.DataStructures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Server.WebServer
{
    public class Server
    {
        WebServiceHost host;

        public DenyoCBWebAPI webserverAPI;

        public Server()
        {
            webserverAPI  = new DenyoCBWebAPI();
        }

        public Server(ref ConcurrentQueue<DataPacket> ReceivedMessagesQRef,ref ConcurrentQueue<DataPacket> PostMessagesQRef)
        {
            webserverAPI = new DenyoCBWebAPI(ref ReceivedMessagesQRef, ref PostMessagesQRef);
        }

        public bool Start()
        {
            try
            {
                host = new WebServiceHost(webserverAPI);

                var behaviour = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                behaviour.InstanceContextMode = InstanceContextMode.Single;

                host.Open();

              //  DenyoCBWebAPI api = new DenyoCBWebAPI(ref _tcpHost);
                return true;
            }catch(Exception ex)
            {
                Console.WriteLine("Error in webHost. Err: " + ex.Message);
                return false;
            }
        }

        public void SendDpToTCPServer(DataPacket dp)
        {
           // _tcpHost.AddMessageToProcessing(dp);
        }
    }
}
