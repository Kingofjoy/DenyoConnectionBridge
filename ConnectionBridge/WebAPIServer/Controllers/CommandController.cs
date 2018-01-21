using Denyo.ConnectionBridge.DataStructures;
using Denyo.ConnectionBridge.Server.TCPServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Web.Http;

namespace WebAPIServer.Controllers
{
    public class CommandController : ApiController
    {
        public TcpClientHandler tcpClientHandler;

        [HttpGet]
        public bool Send(string SenderID, string MsgID, string Message)
        {
            tcpClientHandler = new TcpClientHandler();
            tcpClientHandler.SendToServer_Manual(Message);
            return true;
        }
    }
}
