using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

// Mediaportal references
using TvControl;
using TvDatabase;

using Termors.Services.Tv.MPStreamingInterface;

namespace Termors.Services.Tv.MPStreamingService
{
    public class StreamingServiceImplementation : IStreamingService
    {
        #region Wrapper IStreamingService implementation

        public ChannelList GetChannelList()
        {
            try
            {
                return internal_GetChannelList();
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        public StreamingResult StartStreaming(int channelId, bool record, bool copyToFtp)
        {
            try
            {
                return internal_StartStreaming(channelId, record, copyToFtp);
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        public void StopStreaming(StreamingResult stresult)
        {
            try
            {
                internal_StopStreaming(stresult);
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        public StreamingResult[] GetStreamingStatus()
        {
            try
            {
                return internal_GetStreamingStatus();
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        public StreamingSettings GetConfiguration()
        {
            try
            {
                return internal_GetConfiguration();
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        public void SetConfiguration(StreamingSettings settings)
        {
            try
            {
                internal_SetConfiguration(settings);
            }
            catch (StreamingException se)
            {
                throw new FaultException<StreamingFault>(new StreamingFault(se.Message));
            }
            catch (Exception)
            {
                throw new FaultException<StreamingFault>(new StreamingFault("Unspecified internal error"));
            }
        }

        #endregion

        #region Actual IStreamingService implementation

        protected virtual ChannelList internal_GetChannelList()
        {
            MPCore m = MPCore.Instance;

            IList<ChannelGroup> groups = m.ChannelGroups;
            IList<TvDatabase.Channel> channels = m.Channels;

            ChannelList list = new ChannelList();
            list.ChannelGroups = (from g in groups select g.GroupName).ToArray();

            List<MPStreamingInterface.Channel> returnChannels = new List<MPStreamingInterface.Channel>();

            var tvChannels = from c in channels 
                             where c.IsTv 
                             orderby c.SortOrder
                             select c
                             ;

            string activeGroup = Settings.Default.ChannelGroup.ToLower();
            foreach (TvDatabase.Channel c in tvChannels)
            {
                for (int j = 0; j < c.GroupNames.Count; j++)
                {
                    if (c.GroupNames[j].ToLower() != activeGroup) continue;

                    returnChannels.Add(new MPStreamingInterface.Channel(c.IdChannel, c.DisplayName, c.GroupNames[j]));
                }
            }

            list.Channels = returnChannels.ToArray();
            return list;
        }

        protected virtual StreamingResult internal_StartStreaming(int channelId, bool record, bool copyToFtp)
        {
            string localUrl = Settings.Default.LocalStreamingUrl;
            string remoteUrl = null, recordingUrl = null;

            if (record)
            {
                recordingUrl = "";      // TODO: implement
            }

            StreamingJob job = new StreamingJob(channelId);
            StreamingJob.JobRegistry[channelId] = job;

            job.Started += new StreamingStatusHandler(OnStreamingJobStarted);
            job.Stopped += new StreamingStatusHandler(OnStreamingJobStopped);
            job.Aborted += new StreamingStatusHandler(OnStreamingJobAborted);

            MPStreamingInterface.Channel channelInfo = new MPStreamingInterface.Channel();
            channelInfo.ChannelId = channelId;
            channelInfo.Name = job.ChannelName;

            if (copyToFtp)
            {
                remoteUrl = Settings.Default.RemoteStreamingUrl;

                StreamingFtpJob ftpJob = new StreamingFtpJob(
                    Settings.Default.FtpHost,
                    Settings.Default.FtpUser,
                    Settings.Default.FtpPassword,
                    Settings.Default.FtpRemoteDir,
                    Settings.Default.FtpConcurrentJobs
                    );
                job.NewFiles += new StreamingFilesAvailable(ftpJob.EnqueueFilesFromStreamingJob);
                job.Stopped += new StreamingStatusHandler(ftpJob.StopWithStreamingJob);
                job.Aborted += new StreamingStatusHandler(ftpJob.StopWithStreamingJob);

                if (Settings.Default.TwitterEnabled)
                {
                    job.Started += new StreamingStatusHandler(TweetJobStarted);
                    job.Stopped += new StreamingStatusHandler(TweetJobStopped);
                    job.Aborted += new StreamingStatusHandler(TweetJobAborted);
                }
            }

            job.Start();

            // TODO: work in recording

            StreamingResult result = new StreamingResult(channelInfo, localUrl, remoteUrl, recordingUrl);
            job.AssociatedStreamingResult = result;

            return result;
        }

        private void Tweet(string message, bool uniquify = false)
        {
            TweetSharp.TwitterService svc = new TweetSharp.TwitterService(
                Settings.Default.TwitterConsumerKey,
                Settings.Default.TwitterConsumerSecret
                );

            svc.AuthenticateWith(
                Settings.Default.TwitterAccessToken,
                Settings.Default.TwitterAccessTokenSecret
                );

            TweetSharp.SendTweetOptions options = new TweetSharp.SendTweetOptions();
            options.Status = message;

            if (uniquify)
            {
                long dateval = DateTime.Now.ToBinary();
                options.Status += String.Format(" {0:x}", dateval);
            }

            svc.SendTweet(options);
        }

        private void TweetSafe(string message, bool uniquify = false)
        {
            try
            {
                Tweet(message, uniquify);
            }
            catch (Exception ex)
            {
                // Tweet failed, but don't stop the music
                System.Diagnostics.Trace.WriteLine("Exception sending tweet: " + ex.GetType().FullName + ", " + ex.Message);
            }
        }

        void TweetJobAborted(StreamingJob j)
        {
            Tweet("Streaming of " + j.ChannelName + " aborted abnormally", true);
        }

        void TweetJobStarted(StreamingJob j)
        {
            Tweet("Streaming of " + j.ChannelName + " started on " + Settings.Default.RemoteStreamingUrl, true);
        }

        void TweetJobStopped(StreamingJob j)
        {
            Tweet("Streaming of " + j.ChannelName + " stopped", true);
        }

        protected virtual void internal_StopStreaming(StreamingResult stresult)
        {
            if (StreamingJob.JobRegistry.ContainsKey(stresult.Channel.ChannelId))
            {
                StreamingJob job = StreamingJob.JobRegistry[stresult.Channel.ChannelId];
                StreamingJob.JobRegistry.Remove(stresult.Channel.ChannelId);

                job.Stop();

            }
            else throw new StreamingException("Not currently streaming channel " + stresult.Channel.ChannelId + "; cannot stop, therefore, either");
        }

        protected virtual StreamingResult[] internal_GetStreamingStatus()
        {
            StreamingResult[] results = new StreamingResult[StreamingJob.JobRegistry.Count];

            int i = 0;
            foreach (StreamingJob job in StreamingJob.JobRegistry.Values)
            {
                results[i++] = job.AssociatedStreamingResult;
            }

            return results;
        }

        protected virtual StreamingSettings internal_GetConfiguration()
        {
            StreamingSettings s = new StreamingSettings();

            s.AudioBitrate = Settings.Default.AudioBitrateKbps;
            s.FtpConcurrentJobs = Settings.Default.FtpConcurrentJobs;
            s.MaxVideoBitrate = Settings.Default.VideoMaxBitrateKbps;
            s.SegmentCount = Settings.Default.SegmentCount;
            s.SegmentLength = Settings.Default.SegmentFileLengthSec;
            s.VideoBitrate = Settings.Default.VideoBitrateKbps;
            s.VideoSize = Settings.Default.VideoSize;

            return s;
        }

        protected virtual void internal_SetConfiguration(StreamingSettings value)
        {
            Settings.Default.AudioBitrateKbps = value.AudioBitrate;
            Settings.Default.FtpConcurrentJobs = value.FtpConcurrentJobs;
            Settings.Default.VideoMaxBitrateKbps = value.MaxVideoBitrate;
            Settings.Default.SegmentCount = value.SegmentCount;
            Settings.Default.SegmentFileLengthSec = value.SegmentLength;
            Settings.Default.VideoBitrateKbps = value.VideoBitrate;
            Settings.Default.VideoSize = value.VideoSize;

            Settings.Default.Save();
        }
        

        #endregion

        void OnStreamingJobAborted(StreamingJob j)
        {
            if (StreamingJob.JobRegistry.ContainsKey(j.Channel)) StreamingJob.JobRegistry.Remove(j.Channel);
        }

        void OnStreamingJobStopped(StreamingJob j)
        {
            if (StreamingJob.JobRegistry.ContainsKey(j.Channel)) StreamingJob.JobRegistry.Remove(j.Channel);
        }

        void OnStreamingJobStarted(StreamingJob j)
        {
        }

   }
}
