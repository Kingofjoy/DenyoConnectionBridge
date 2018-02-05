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

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "TransactMin/{Sender}/{Receiver}/{Message}")]
        string TransactMin(string Sender, string Receiver, string Message);
        
        // Sample
        /*

        Auto:
        http://XX.XX.XX.XXX/DenyoCBWebAPI/Transact/Data={"SenderType":2,"SenderID":"APPTST1","RecepientType":1,"RecepientID":"GEN0002","TimeStamp":"2018-01-24T23:47:23.3797551+08:00","MsgID":"32","Message":"01 06 0b cb 00 02 7b d1","IsManualCmd":true}
        Off:
        http://XX.XX.XX.XXX/DenyoCBWebAPI/Transact/Data={"SenderType":2,"SenderID":"APPTST1","RecepientType":1,"RecepientID":"GEN0002","TimeStamp":"2018-01-24T23:47:23.3797551+08:00","MsgID":"32","Message":"01 06 0b cb 00 00 fa 10","IsManualCmd":true}
        Manual:
        http://XX.XX.XX.XXX/DenyoCBWebAPI/Transact/Data={"SenderType":2,"SenderID":"APPTST1","RecepientType":1,"RecepientID":"GEN0002","TimeStamp":"2018-01-24T23:47:23.3797551+08:00","MsgID":"32","Message":"01 06 0b cb 00 01 3b d0","IsManualCmd":true}

        */

    }
}
