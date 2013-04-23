using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;


namespace Termors.Services.Tv.MPStreamingService
{
    public partial class WindowsService : ServiceBase
    {
        private ServiceHost m_serviceHost = null;

        public WindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (m_serviceHost != null) OnStop();

            m_serviceHost = new ServiceHost(typeof(StreamingServiceImplementation));
            m_serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (m_serviceHost != null)
            {
                m_serviceHost.Close();
                m_serviceHost = null;
            }

            foreach (StreamingJob j in StreamingJob.JobRegistry.Values) try
                {
                    j.Stop();
                }
                catch { }
        }

        internal void Start(string[] args)
        {
            OnStart(args);
        }

        internal void StopService()
        {
            OnStop();
        }
    }
}
