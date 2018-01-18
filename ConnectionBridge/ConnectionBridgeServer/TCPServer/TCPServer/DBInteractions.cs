using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data;

namespace Denyo.ConnectionBridge.Server.TCPServer
{
    class DBInteractions
    {
        public bool UpdateMonitoringStatus()
        {
            try
            {

                return true;
            }
            catch(Exception ex)
            {

                return false;
            }
        }
    }
}
