using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// Mediaportal references
using TvControl;
using TvDatabase;

using Termors.Services.Tv.MPStreamingInterface;


namespace Termors.Services.Tv.MPStreamingService
{
    internal delegate void StreamingStatusHandler(StreamingJob j);
    internal delegate void StreamingFilesAvailable(StreamingJob j, string[] fileNames);

    internal class StreamingJob
    {
        private readonly int m_channelId;
        private readonly string m_sChannelName;

        private Process m_procEncoder = null;
        private DateTime m_dtLastWatchdogEvent = DateTime.Now;
        private bool m_bMonitorFiles = false;

        private StreamingResult m_result = null;

        private LinkedList<string> m_listLast100LinesOfErrors = new LinkedList<string>();


        public StreamingJob(int channelId)
        {
            m_channelId = channelId;
            m_sChannelName = MPCore.Instance.GetChannelName(channelId);
        }

        public int Channel
        {
            get { return m_channelId; }
        }

        public string ChannelName
        {
            get { return m_sChannelName; }
        }

        public StreamingResult AssociatedStreamingResult
        {
            get { return m_result; }
            set { m_result = value; }
        }

        /// <summary>
        /// Registry for all running jobs, to prevent garbage collection
        /// </summary>
        public static readonly IDictionary<int, StreamingJob> JobRegistry = new Dictionary<int, StreamingJob>();

        public event StreamingStatusHandler Started;
        public event StreamingStatusHandler Stopped;
        public event StreamingStatusHandler Aborted;

        public event StreamingFilesAvailable NewFiles;

        public void Start()
        {
            try
            {
                InitFileSystem();

                StartMonitoringForFiles();
                StartStreaming();
                StartEncoding();
            }
            catch
            {
                AbortJob();
                throw;
            }

            if (Started != null) Started(this);
        }

        public void Stop()
        {
            Exception e = null;

            try
            {
                StopEncoding();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            try
            {
                StopStreaming();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            try
            {
                StopMonitoringForFiles();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            if (e != null)
            {
                AbortJob();
                throw e;
            }

            if (Stopped != null) Stopped(this);
        }

        public string[] MostRecentEncoderErrorOutput
        {
            get
            {
                lock (m_listLast100LinesOfErrors)
                {
                    return m_listLast100LinesOfErrors.ToArray();
                }
            }
        }

        /// <summary>
        /// Private abort method called when something is wrong (e.g. ffmpeg stopped)
        /// </summary>
        private void AbortJob()
        {
            StopEncoding();
            StopStreaming();
            StopMonitoringForFiles();

            if (Aborted != null) Aborted(this);
        }

        #region Start and stop sub methods for streaming, encoding, etcetera

        /// <summary>
        /// Delete the current files, so FFMPeg can start with a clean slate
        /// </summary>
        private void InitFileSystem()
        {
            string[] qualifyingfiles = Directory.GetFiles(
                Settings.Default.OutputDirectory,
                String.Format("*{0}*", Settings.Default.BaseFileName),
                SearchOption.TopDirectoryOnly
                );

            // Delete each of the old files
            foreach (string file in qualifyingfiles) File.Delete(file);
        }

        private void StartMonitoringForFiles()
        {
            m_bMonitorFiles = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(MonitorFiles));
        }

        private void StopMonitoringForFiles()
        {
            m_bMonitorFiles = false;
        }

        private void StartStreaming()
        {
            TvResult result = MPCore.Instance.StartTimeShift(m_channelId);
            if (result != TvResult.Succeeded) throw new StreamingException("Start streaming failed, reason: " + result.ToString());
        }

        private void StopStreaming()
        {
            MPCore.Instance.StopTimeShift();
        }

        private void StartEncoding()
        {
            int videoStream, audioStream;

            // Clear error output
            m_listLast100LinesOfErrors.Clear();

            FFProbeGetStreamIndexes(out videoStream, out audioStream);
            StartFFMpeg(videoStream, audioStream);
        }

        private void StopEncoding()
        {
            if (m_procEncoder != null)
            {
                m_procEncoder.Exited -= new EventHandler(OnProcess_Exit_Watchdog);
                if (! m_procEncoder.HasExited) m_procEncoder.Kill();
                m_procEncoder = null;
            }
        }

        #endregion

        #region FFMpeg and FFProbe handling

        private void FFProbeGetStreamIndexes(out int videoStream, out int audioStream)
        {
            string ffProbeCommandLine = String.Format(
                Settings.Default.FFProbeCommandLine,
                MPCore.Instance.RtspUrl
                );

            ProcessStartInfo psi = new ProcessStartInfo(
                Settings.Default.FFProbePath,                // Directory of ffmpeg binary
                ffProbeCommandLine
                );

            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardError = psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            Process pFFProbe = null;
            try
            {
                pFFProbe = Process.Start(psi);

                string xmlOutput = pFFProbe.StandardOutput.ReadToEnd();
                pFFProbe.WaitForExit();
                pFFProbe = null;

                FFProbeXmlOutput ffProbe = new FFProbeXmlOutput(xmlOutput);
                if (ffProbe.AudioStreams.Count == 0) throw new StreamingException("No audio stream found for channel");
                if (ffProbe.VideoStreams.Count == 0) throw new StreamingException("No video stream found for channel");

                //TODO: select best stream
                videoStream = ffProbe.VideoStreams[0].StreamIndex;
                audioStream = ffProbe.AudioStreams[0].StreamIndex;
            }
            catch
            {
                if (pFFProbe != null) try { pFFProbe.Kill(); }
                    catch { }
                throw;
            }

        }

        private void StartFFMpeg(int videoStream, int audioStream)
        {
            string ffmpegCommandLine = ConstructFFMpegCommandLine(videoStream, audioStream);

            ProcessStartInfo psi = new ProcessStartInfo(
                Settings.Default.FFMpegPath,                // Directory of ffmpeg binary
                ffmpegCommandLine
                );

            psi.WorkingDirectory = Settings.Default.OutputDirectory;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardError = psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            m_procEncoder = Process.Start(psi);

            m_procEncoder.Exited += new EventHandler(OnProcess_Exit_Watchdog);
            m_procEncoder.ErrorDataReceived += new DataReceivedEventHandler(OnProcessError);
            m_procEncoder.OutputDataReceived += new DataReceivedEventHandler(OnProcessOutput);

            m_procEncoder.BeginErrorReadLine();
            m_procEncoder.BeginOutputReadLine();

            StartProcessWatchdog();
        }

        private static string ConstructFFMpegCommandLine(int videoStream, int audioStream)
        {
            string ffmpegCommandLine = String.Format(
                Settings.Default.FFMpegCommandLine,         // Template command line with gaps (e.g. {0})
                MPCore.Instance.RtspUrl,                    // Source of streaming
                Settings.Default.BaseFileName,              // Base file name (e.g. "stream")
                videoStream,
                audioStream,
                Settings.Default.VideoBitrateKbps,
                Settings.Default.AudioBitrateKbps,
                Settings.Default.VideoSize,
                Settings.Default.SegmentFileLengthSec,
                Settings.Default.SegmentCount,
                Settings.Default.VideoMaxBitrateKbps
                );

            return ffmpegCommandLine;
        }

        private void StartProcessWatchdog()
        {
            m_dtLastWatchdogEvent = DateTime.Now;

            ThreadPool.QueueUserWorkItem(new WaitCallback(EncoderWatchDog));
        }

        void OnProcessOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            System.Diagnostics.Trace.WriteLine(e.Data);
        }

        void OnProcessError(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            // Keep the last 100 lines of error output
            lock (m_listLast100LinesOfErrors)
            {
                m_listLast100LinesOfErrors.AddLast(e.Data);

                while (m_listLast100LinesOfErrors.Count > 100) m_listLast100LinesOfErrors.RemoveFirst();
            }

            if (m_procEncoder != null && e.Data.Contains("fps=")) lock (m_procEncoder)
                {
                    // Based on output, process still running ok
                    // Satisfy watchdog
                    m_dtLastWatchdogEvent = DateTime.Now;
                }
        }

        void OnProcess_Exit_Watchdog(object sender, EventArgs e)
        {
            // Encoder exited for some reason. Abort!
            m_procEncoder = null;
            AbortJob();
        }

        #endregion

        #region Watchdog and threadpool processes

        private void EncoderWatchDog(object bogus)
        {
            Thread.Sleep(20000);

            if (m_procEncoder == null) return;

            lock (m_procEncoder)
            {
                if (DateTime.Now.Subtract(m_dtLastWatchdogEvent).TotalSeconds > 10.0)
                {
                    AbortJob();
                }
                else ThreadPool.QueueUserWorkItem(new WaitCallback(EncoderWatchDog));
            }
        }

        private void MonitorFiles(object bogus)
        {
            if (m_bMonitorFiles)
            {
                // Does anyone care?
                if (NewFiles != null)
                {
                    string[] modifiedTsFiles = FindModifiedTsFiles();

                    if (modifiedTsFiles.Length > 0)
                    {
                        // Yes, modified .ts files found
                        // Clear the archive bit on all of them
                        foreach (string file in modifiedTsFiles) File.SetAttributes(file, File.GetAttributes(file) ^ FileAttributes.Archive);

                        // Also always add the .m3u8 file
                        string[] allFiles = new string[modifiedTsFiles.Length + 1];
                        Array.Copy(modifiedTsFiles, 0, allFiles, 0, modifiedTsFiles.Length);
                        allFiles[allFiles.Length - 1] = Path.Combine(Settings.Default.OutputDirectory, Settings.Default.BaseFileName + ".m3u8");

                        NewFiles(this, allFiles);
                    }

                    Thread.Sleep(1000);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(MonitorFiles));
                }
            }
        }

        #endregion

        private static string[] FindModifiedTsFiles()
        {
            // Find modified .ts files
            string[] filesInDir = Directory.GetFiles(
                Settings.Default.OutputDirectory,
                String.Format("*{0}*.ts", Settings.Default.BaseFileName),
                SearchOption.TopDirectoryOnly
                );
            
            // Get all files with the attribute bit cleared, that can be exclusively locked (i.e. are not being written to)
            var modifiedFiles = from f in filesInDir where ((File.GetAttributes(f) & FileAttributes.Archive) != 0 && CanGetExclusiveLock(f)) select f;

            return modifiedFiles.ToArray<string>();
        }

        private static bool CanGetExclusiveLock(string f)
        {
            bool bLockable = false;

            try
            {
                using (FileStream fsExclusive = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // File successfully locked.
                    bLockable = true;
                }
            }
            catch
            {
                // File not lockable
            }

            return bLockable;
        }


    }
}
