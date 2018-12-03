//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Denyo.ConnectionBridge.Client
{
    
    [RoutePrefix("CBClientApi/status")]
    public class APIController : ApiController
    {
        Dictionary<string, string> statusDicTemp = new Dictionary<string, string>();
        [Route("getall")]
        public Dictionary<string,string> GetAllStatus()
        {

            if (Logger.StatusBook.Count == 0)
            {
                statusDicTemp.Add("Service", "InActive");
                statusDicTemp.Add("Sys Time", DateTime.Now.ToString());
                return statusDicTemp;

            }
            else
                return Logger.StatusBook;
        }

        public IEnumerable<string> GetAllItems()
        {
            return new List<string> { Logger.WorkerHeartBeat.ToString() };
        }

    }
}
