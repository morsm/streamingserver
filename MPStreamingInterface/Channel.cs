using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [DataContract]
    public class Channel
    {
        private int m_ChannelId;
        private string m_Name;
        private string m_ChannelGroup;

        public Channel()
        {
            ChannelId = 0;
            Name = ChannelGroup = "";
        }

        public Channel(int channelId, string name, string channelgroup)
        {
            ChannelId = channelId;
            Name = name;
            ChannelGroup = channelgroup;
        }

        [DataMember]
        public int ChannelId
        {
            get { return m_ChannelId; }
            set { m_ChannelId = value; }
        }

        [DataMember]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [DataMember]
        public string ChannelGroup
        {
            get { return m_ChannelGroup; }
            set { m_ChannelGroup = value; }
        }
    }
}
