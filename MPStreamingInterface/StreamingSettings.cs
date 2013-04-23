using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [DataContract]
    public class StreamingSettings
    {
        public StreamingSettings()
        {
        }
        
        [DataMember]
        private ushort m_videoBitrate = 400;
        public ushort VideoBitrate
        {
            get { return m_videoBitrate; }
            set { m_videoBitrate = value; }
        }

        [DataMember]
        private ushort m_maxVideoBitrate = 600;
        public ushort MaxVideoBitrate
        {
            get { return m_maxVideoBitrate; }
            set { m_maxVideoBitrate = value; }
        }

        [DataMember]
        private ushort m_audioBitrate = 48;
        public ushort AudioBitrate
        {
            get { return m_audioBitrate; }
            set { m_audioBitrate = value; }
        }

        [DataMember]
        private ushort m_ftpConcurrentJobs = 2;
        public ushort FtpConcurrentJobs
        {
            get { return m_ftpConcurrentJobs; }
            set { m_ftpConcurrentJobs = value; }
        }

        [DataMember]
        private ushort m_segmentLength = 5;
        public ushort SegmentLength
        {
            get { return m_segmentLength; }
            set { m_segmentLength = value; }
        }

        [DataMember]
        private ushort m_segmentCount = 60;
        public ushort SegmentCount
        {
            get { return m_segmentCount; }
            set { m_segmentCount = value; }
        }

        [DataMember]
        private string m_videoSize = "360x288";
        public string VideoSize
        {
            get { return m_videoSize; }
            set {
                ArgumentException e = new ArgumentException("Format has to be in format <width>x<height>, e.g. 720x576");

                // Ensure correct format
                string format = value.ToLower();
                int xIndex = format.IndexOf('x');

                if (xIndex < 0) throw e;
                try
                {
                    int width = Int32.Parse(format.Substring(0, xIndex));
                    int height = Int32.Parse(format.Substring(xIndex + 1));
                }
                catch
                {
                    throw e;
                }

                m_videoSize = format;
            
            }
        }



    }
}
