using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using Denyo.ConnectionBridge.DataStructures;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Denyo.ConnectionBridge.Server.WebServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DenyoCBWebAPI" in both code and config file together.
    public class DenyoCBWebAPI : IDenyoCBWebAPI
    {

        private static readonly DenyoCBWebAPI instance = new DenyoCBWebAPI();

        ConcurrentQueue<DataPacket> ReceivedMessages
        {
            get; set;
        }
        ConcurrentQueue<DataPacket> PostMessages
        {
            get; set;
        }
        ConcurrentQueue<DataPacket> ProcessedMessages
        {
            get; set;
        }

        //static DenyoCBWebAPI()
        //{

        //}
        public DenyoCBWebAPI(ref ConcurrentDictionary<string, ConcurrentQueue<DataPacket>> MessageQueuesRef)
        {
            ReceivedMessages = MessageQueuesRef["ReceivedMessages"];
            PostMessages = MessageQueuesRef["PostMessages"];
            ProcessedMessages = MessageQueuesRef["ProcessedMessages"];
        }
        public DenyoCBWebAPI()
        {
            ReceivedMessages = new ConcurrentQueue<DataPacket>();
            PostMessages = new ConcurrentQueue<DataPacket>();
        }

        //public static DenyoCBWebAPI Instance
        //{
        //    get
        //    {
        //        return instance;
        //    }
        //}

        //public DenyoCBWebAPI(ref TCPServer.Server server)
        //{
        //    tcpServer = server;
        //}
        public string DoWork(string strInput)
        {
            return "Input Data Received: " + strInput;
        }

        public string Transact(string dpInputString)
        {
            DataPacket dpInputPacket = new DataPacket();
            try
            {
                dpInputPacket = JsonConvert.DeserializeObject<DataPacket>(dpInputString);
                dpInputPacket.Message = "ACK. " + dpInputPacket.Message;
                //dpInputPacket.TimeStamp = DateTime.Now;
                //string strSenderName = dpInputPacket.RecepientID;
                //string strReceiverName = dpInputPacket.SenderID;

                //dpInputPacket.SenderID = strSenderName;
                //dpInputPacket.RecepientID = strReceiverName;

                ReceivedMessages.Enqueue(dpInputPacket);
            }
            catch(Exception ex)
            {
                dpInputPacket.Message = "Error while processing input." + ex.Message;
            }
            return JsonConvert.SerializeObject(dpInputPacket);
        }
    }
}
