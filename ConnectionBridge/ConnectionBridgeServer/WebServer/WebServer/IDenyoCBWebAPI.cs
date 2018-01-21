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

    }
}
