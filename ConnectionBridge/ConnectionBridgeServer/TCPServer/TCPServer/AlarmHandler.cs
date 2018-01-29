using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Server.TCPServer
{
    class AlarmHandler
    {
        public Dictionary<string, int> AlarmMaster = new Dictionary<string, int>();
        public Dictionary<string, List<string>> Alarams = new Dictionary<string, List<string>>();


    }
}
