using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using Denyo.ConnectionBridge.DataStructures; 

namespace Denyo.ConnectionBridge.Server.WebServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDenyoCBWebAPI" in both code and config file together.
    [ServiceContract]
    public interface IDenyoCBWebAPI
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "DoWork/{strInput}")]
        string DoWork(string strInput);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "Transact/Data={dpInput}")]
        string Transact(string dpInput);

        // Sample
        // http://13.58.37.241/DenyoCBWebAPI/Transact/Data=%7B%22SenderType%22:1,%22SenderID%22:%22VINCENT%22,%22RecepientType%22:0,%22RecepientID%22:%22MINIPC001%22,%22TimeStamp%22:%222018-01-18T23:47:23.3797551+08:00%22,%22MsgID%22:%2232%22,%22Message%22:%22TEST%20STRING4%22,%22IsManualCmd%22:false%7D


    }
}
