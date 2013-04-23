using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Mediaportal references
using TvControl;
using TvDatabase;


namespace Termors.Services.Tv.MPStreamingService
{
    /// <summary>
    /// Singleton connection to MediaPortal TvService
    /// </summary>
    class MPCore
    {
        private static MPCore singleton = null;

        private IUser me;
        private IList<ChannelGroup> m_chGroups;
        private IList<Channel> m_channels;
        private VirtualCard card;


        private MPCore()
        {
            Initialize();
        }

        private void Initialize()
        {
            RemoteControl.HostName = Settings.Default.TvServerHost;
            me = new User();

            GetChannelGroups();
            GetChannels();
        }

        public void StopTimeShift()
        {
            if (card != null) card.StopTimeShifting();
            card = null;
        }

        public TvResult StartTimeShift(int channel)
        {
            if (card != null)
            {
                // Currently, only one timeshift at a time can run
                return TvResult.AllCardsBusy;
            }

            return RemoteControl.Instance.StartTimeShifting(ref me, channel, out card);
        }

        public bool IsTimeshifting
        {
            get { return (card != null && card.IsTimeShifting); }
        }

        public string RtspUrl
        {
            get
            {
                if (!IsTimeshifting) throw new StreamingException("No RTSP Url available, not streaming");

                return card.RTSPUrl;
            }
        }
        

        public string GetChannelName(int channelId)
        {
            Channel ch = TvDatabase.Channel.Retrieve(channelId);
            return ch.DisplayName;
        }

        private void GetChannels()
        {
            m_channels = TvDatabase.Channel.ListAll();
        }

        private void GetChannelGroups()
        {
            m_chGroups = TvDatabase.ChannelGroup.ListAll();
        }

        public static MPCore Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new MPCore();
                }
                return singleton;
            }
        }

        public IList<Channel> Channels
        {
            get
            {
                return m_channels;
            }
        }

        public IList<ChannelGroup> ChannelGroups
        {
            get
            {
                return m_chGroups;
            }
        }

    }
}
