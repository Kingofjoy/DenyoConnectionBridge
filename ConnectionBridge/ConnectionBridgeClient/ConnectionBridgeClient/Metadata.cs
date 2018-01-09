using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Denyo.ConnectionBridge.DataStructures;

namespace Denyo.ConnectionBridge.Client
{
    public static class Metadata
    {
        public static string AppID { get; set; }

        public static string AppType { get; set; }

        public static string ServerIP {get;set;}

        public static int ServerPort { get; set; }

        public static string PublicNetURL { get; set; }

        public static string ServerURL { get; set; }

        public static string AuthToken{ get; set; }

        public static string PreferredCOMPort { get; set; } 

        public static string PreferredBaudRate { get; set; }

        public static int TimerInterval { get; set; }

        public static List<HexaInput> InputDictionary = new List<HexaInput>();

    }

    public enum CommunicationMode
    {
        HEXA,
        TEXT
    }
}
