using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Termors.Services.Tv.MPStreamingService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower().Equals("-standalone")) StartAsProgram(args); else StartAsService();
        }

        private static void StartAsService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new WindowsService() 
			};
            ServiceBase.Run(ServicesToRun);
        }

        private static void StartAsProgram(string[] args)
        {
            WindowsService svc = new WindowsService();

            svc.Start(args);

            // Wait for telnet connection to port
            System.Net.Sockets.TcpListener tcp = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, Settings.Default.DebugInterruptTcpPort);
            tcp.Start();
            tcp.AcceptSocket();

            svc.StopService();
        }

    }
}
