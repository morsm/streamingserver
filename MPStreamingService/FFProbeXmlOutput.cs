using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Termors.Services.Tv.MPStreamingService
{
    class FFProbeXmlOutput
    {
        private XPathDocument m_xpathDoc = null;
        private List<FFVideoStream> m_videoStreams = new List<FFVideoStream>();
        private List<FFAudioStream> m_audioStreams = new List<FFAudioStream>();


        public FFProbeXmlOutput(string xml)
        {
            LoadXml(xml);
        }

        public void LoadXml(string xml)
        {
            StringReader reader = new StringReader(xml);
            m_xpathDoc = new XPathDocument(reader);

            m_videoStreams.Clear();
            m_audioStreams.Clear();

            GetStreamInfo();
        }

        public IList<FFVideoStream> VideoStreams
        {
            get
            {
                return m_videoStreams;
            }
        }

        public IList<FFAudioStream> AudioStreams
        {
            get
            {
                return m_audioStreams;
            }
        }

        private void GetStreamInfo()
        {
            XPathNavigator nav = m_xpathDoc.CreateNavigator();

            XPathNodeIterator videoIter = nav.Select("/ffprobe/streams/stream[@codec_type=\"video\"]");
            XPathNodeIterator audioIter = nav.Select("/ffprobe/streams/stream[@codec_type=\"audio\"]");

            while (videoIter.MoveNext())
            {
                m_videoStreams.Add(
                    new FFVideoStream(
                        Int32.Parse(videoIter.Current.GetAttribute("index","").ToString()),
                        Int32.Parse(videoIter.Current.GetAttribute("width", "").ToString()), 
                        Int32.Parse(videoIter.Current.GetAttribute("height", "").ToString()))
                    );
            }

            while (audioIter.MoveNext())
            {
                string language = "";

                XPathNodeIterator langIter = audioIter.Current.Select("tag[@key=\"language\"]");
                if (langIter.MoveNext()) language = langIter.Current.GetAttribute("value", "");

                m_audioStreams.Add(
                    new FFAudioStream(
                        Int32.Parse(audioIter.Current.GetAttribute("index", "").ToString()),
                        language)
                    );
            }
        }
    }

    class FFStream
    {
        private readonly int m_index;

        public FFStream(int index)
        {
            m_index = index;
        }

        public int StreamIndex
        {
            get { return m_index; }
        }

    }

    class FFVideoStream : FFStream
    {
        private readonly int m_width, m_height;

        public FFVideoStream(int index, int width, int height)
            : base(index)
        {
            m_width = width;
            m_height = height;
        }

        public int Width { get { return m_width; } }
        public int Height { get { return m_height; } }

    }

    class FFAudioStream : FFStream
    {
        private readonly string m_language;

        public FFAudioStream(int index, string language)
            : base(index)
        {
            m_language = language;
        }

        public string Language { get { return m_language; } }
    }

}
