using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics.Modules;
using Nancy.Hosting.Self;
using Nancy.ViewEngines;

namespace VersaTrackerUI
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri("http://localhost:8888");

            var hostConfigs = new HostConfiguration
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            using (var host = new NancyHost(hostConfigs, uri))
            {
                host.Start();
                Console.WriteLine("Server running");
                Console.ReadLine();
            }
        }
    }

    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get("/{item}", parameters => { return "Hello!"; });

            /*
            Get["/"] = x =>
            {
                var model = new ConfigStatusModel
                {
                    Config = new ConfigInfo()
                };
                return View["index.html", model];
            };
            */
        }
    }
}
