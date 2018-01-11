using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.DataStructures
{
    class Definitions
    {
    }

    [Serializable]
    public class HexaInput
    {
        public string Raw { get; set; }

        public string Hexa { get; set; }

        public string Name { get; set; }

        public string P1 { get; set; }

        public string P2 { get; set; }

        public string P3 { get; set; }

        public string P4 { get; set; }

        public string P5 { get; set; }

        public string PX { get; set; }


    }

    public enum LogType
    {
        Message,
        Warning,
        Error
    }

    [Serializable]
    public enum AppType
    {
        Server, Client, Moderator, Listener
    }

    [Serializable]
    public class DataPacket
    {
        public AppType SenderType { get; set; }

        public string SenderID { get; set; }

        public AppType RecepientType { get; set; }

        public string RecepientID { get; set; }

        public DateTime TimeStamp { get; set; }

        public string MsgID { get; set; }

        public string Message { get; set; }

        public bool IsManualCmd { get; set; }
    }
}
