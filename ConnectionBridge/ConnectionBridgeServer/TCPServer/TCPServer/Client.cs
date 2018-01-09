using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Server.TCPServer
{
    class Client
    {
        public TcpClient Instance { get; set; }

        public NetworkStream Stream { get; set; }

        public string Name { get; set; }

        public DateTime LastActive { get; set; }

        public string AuthToken { get; set; }
    }
}
