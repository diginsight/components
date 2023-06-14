using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareManager.Service
{
    static class Program
    {
        public static Type T = typeof(Program);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //Thread.Sleep(10000);
            using (var sec = TraceManager.GetCodeSection(T))
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new SoftwareManagerService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
