using System;
using System.Reflection;
using Daemonizer;
using System.ServiceProcess;

namespace DaemonizerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            ProcessService service = new ProcessService();
            service.StartService(new string[0]);

            System.Threading.Thread.Sleep(60 * 60000);
            service.StopService();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProcessService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
