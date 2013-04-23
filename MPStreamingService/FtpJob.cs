using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Termors.Services.Tv.MPStreamingService
{
    internal class FtpJob : IDisposable
    {
        protected readonly string m_sFtpHost, m_sFtpUser, m_sFtpPassword, m_sFtpDir;
        protected readonly Thread[] m_threads;
        protected Queue<string> m_qFiles = new Queue<string>();

        private AutoResetEvent m_eventNewFiles = new AutoResetEvent(false);
        private ManualResetEvent m_eventQuit = new ManualResetEvent(false);

        public FtpJob(string ftpHost, string user = "ftp", string password = "john@doe.com", string remoteDirectory = "/", int numberOfThreads = 2)
        {
            m_sFtpDir = remoteDirectory;
            m_sFtpPassword = password;
            m_sFtpUser = user;
            m_sFtpHost = ftpHost;

            m_threads = new Thread[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                m_threads[i] = new Thread(new ThreadStart(FileThreadProc));
                m_threads[i].Start();
            }
        }

        public void QueueFile(string file)
        {
            lock (m_qFiles)
            {
                m_qFiles.Enqueue(file);
                SetFileAvailableEvent();
            }
        }

        public void QueueFile(string[] files)
        {
            foreach (string file in files) QueueFile(file);
        }

        private void SetFileAvailableEvent()
        {
            m_eventNewFiles.Set();
        }

        protected virtual void FileThreadProc()
        {
            WaitHandle[] handles = new WaitHandle[] { m_eventQuit, m_eventNewFiles };

            while (WaitHandle.WaitAny(handles) != 0)
            {

                string file;
                lock (m_qFiles)
                {
                    if (m_qFiles.Count == 0) continue;  // Queue empty. Should not occur.

                    // Get the first file out
                    file = m_qFiles.Dequeue();

                    System.Diagnostics.Trace.WriteLine("Pushing file " + file + ", " + m_qFiles.Count + " files remaining in queue");

                    // More files? then retrigger the event for the benefit of other threads
                    if (m_qFiles.Count > 0) SetFileAvailableEvent();
                }

                try
                {
                    PushFileFtp(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(
                        String.Format("Exception {0}, message {1} during pushing of file {2}. Requeueing...",
                        ex.GetType().Name,
                        ex.Message,
                        file)
                        );

                    QueueFile(file);
                }

            }            
        }

        private static void PushFileFtp(string f)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(
                "ftp://" + Settings.Default.FtpHost
                + Settings.Default.FtpRemoteDir
                + "/" + Path.GetFileName(f)
                );

            request.UseBinary = true;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(Settings.Default.FtpUser, Settings.Default.FtpPassword);

            byte[] data;
            using (FileStream fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }

            DateTime start = DateTime.Now;
            using (Stream ftpStream = request.GetRequestStream())
            {
                ftpStream.Write(data, 0, data.Length);
            }
            DateTime end = DateTime.Now;

            System.Diagnostics.Trace.WriteLine(String.Format("File {1} uploaded, speed: {0} kB/s", 
                Convert.ToInt32(Math.Round(Convert.ToDouble(data.Length) / 1.024 / end.Subtract(start).TotalMilliseconds)),
                Path.GetFileName(f)
                ));

        }

        public void Dispose()
        {
            m_eventQuit.Set();
        }
    }

    internal class StreamingFtpJob : FtpJob
    {
        public StreamingFtpJob(string ftpHost, string user = "ftp", string password = "john@doe.com", string remoteDirectory = "/", int numberOfThreads = 2)
            : base(ftpHost, user, password, remoteDirectory, numberOfThreads)
        {
        }

        public void EnqueueFilesFromStreamingJob(StreamingJob j, string[] files)
        {
            QueueFile(files);
        }

        public void StopWithStreamingJob(StreamingJob j)
        {
            Dispose();
        }
    }
}
