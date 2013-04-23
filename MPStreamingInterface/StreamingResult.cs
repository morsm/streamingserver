using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [DataContract]
    public class StreamingResult
    {
        [DataMember]
        private readonly Channel m_channel;
        [DataMember]
        private readonly string m_sLocalUrl;
        [DataMember]
        private readonly string m_sRemoteUrl;
        [DataMember]
        private readonly string m_sRecordingUrl;

        public StreamingResult(Channel channel, string localUrl, string remoteUrl = null, string recordingUrl = null)
        {
            m_channel = channel;
            m_sLocalUrl = localUrl;
            m_sRemoteUrl = remoteUrl;
            m_sRecordingUrl = remoteUrl;
        }

        public Channel Channel
        {
            get { return m_channel; }
        }

        public string LocalVideoUrl
        {
            get { return m_sLocalUrl; }
        }

        public bool HasRemoteCopy
        {
            get { return m_sRemoteUrl != null; }
        }

        public bool HasRecording
        {
            get { return m_sRecordingUrl != null; }
        }

        public string RemoteVideoUrl
        {
            get { return m_sRemoteUrl; }
        }

        public string RecordingUrl
        {
            get { return m_sRecordingUrl; }
        }

    }
}
