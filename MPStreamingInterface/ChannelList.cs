using System;
using System.Collections.Generic;
using System.Linq; 
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [DataContract]
    public class ChannelList
    {
        private string[] m_strarrChannelGroups;
        private Channel[] m_arrChannels;

        [DataMember]
        public string[] ChannelGroups
        {
            get { return m_strarrChannelGroups; }
            set { m_strarrChannelGroups = value; }
        }

        [DataMember]
        public Channel[] Channels
        {
            get
            {
                return m_arrChannels;
            }
            set { m_arrChannels = value; }
        }
    }
}
