using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Host.HttpListener;

namespace Denyo.ConnectionBridge.Client
{
    class APIStartup
    {
        Type valuesControllerType = typeof(Denyo.ConnectionBridge.Client.APIController);
        public void Configuration(IAppBuilder appBuilder)
        {
            

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            //  Enable attribute based routing
            //  http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ConnectionBridge",
                routeTemplate: "CBClientApi/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Clear();
            config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());

            appBuilder.UseWebApi(config);
            
        }
    }

    public class WebAPIHandler
    {
        public string baseAddress = "http://localhost:9080/";
        private IDisposable _server = null;
        public bool isRunning = false;

        public bool Start()
        {
            try
            {
                _server = Microsoft.Owin.Hosting.WebApp.Start<APIStartup>(url: baseAddress);
                
                isRunning = true;
                return true;
            }catch(Exception ex)
            {
                Logger.Log("WebAPIHandler Start Error: "+ ex.Message, ex);
                return false;
            }
        }

        public bool Stop()
        {
            try
            {
                if (_server != null)
                {
                    _server.Dispose();
                }
                return true;
            }
            catch(Exception ex2)
            {
                Logger.Log("WebAPIHandler Stop Error: " + ex2.Message, ex2);
                return false;
            }
            
        }
    }
}
