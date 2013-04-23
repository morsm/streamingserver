using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

using Termors.Services.Tv.MPStreamingInterface;


namespace StreamingApp
{
    public class ServiceConnection
    {
        private ChannelList m_channelList = null;
        private StreamingServiceReference.StreamingServiceClient svc = new StreamingServiceReference.StreamingServiceClient();

        public static readonly string CHANNEL_CACHE_ID = "ChannelList";

        public ServiceConnection()
        {
            // Attempt to retrieve channel list from Http cache
            m_channelList = (ChannelList)HttpRuntime.Cache[CHANNEL_CACHE_ID];
        }

        public Channel[] GetChannels()
        {
            try
            {

                if (m_channelList == null)
                {
                    m_channelList = svc.GetChannelList();
                    HttpRuntime.Cache[CHANNEL_CACHE_ID] = m_channelList;
                }
            }
            catch (FaultException<StreamingFault> e)
            {
                throw new StreamingWebException(e.Detail.Error);
            }
            catch (EndpointNotFoundException)
            {
                throw new StreamingWebException("Could not connect to backend streaming server.");
            }

            return m_channelList.Channels;
        }

        public void StartStreaming(int channel, bool bFTP = false, bool bRecording = false)
        {
            try
            {
                svc.StartStreaming(channel, false, bFTP);
            }
            catch (FaultException<StreamingFault> e)
            {
                throw new StreamingWebException(e.Detail.Error);
            }
            catch (EndpointNotFoundException)
            {
                throw new StreamingWebException("Could not connect to backend streaming server.");
            }
        }

        public void StopStreaming(int channel)
        {
            try
            {
                StreamingResult result = new StreamingResult(new Channel(channel, "", ""), "");
                StopStreaming(result);
            }
            catch (FaultException<StreamingFault> e)
            {
                throw new StreamingWebException(e.Detail.Error);
            }
            catch (EndpointNotFoundException)
            {
                throw new StreamingWebException("Could not connect to backend streaming server.");
            }
        }

        public void StopStreaming(StreamingResult result)
        {
            try
            {
                svc.StopStreaming(result);
            }
            catch (FaultException<StreamingFault> e)
            {
                throw new StreamingWebException(e.Detail.Error);
            }
            catch (EndpointNotFoundException)
            {
                throw new StreamingWebException("Could not connect to backend streaming server.");
            }
        }

        public StreamingResult[] GetCurrentStreamingResult()
        {
            try
            {
                return svc.GetStreamingStatus();
            }
            catch (FaultException<StreamingFault> e)
            {
                throw new StreamingWebException(e.Detail.Error);
            }
            catch (EndpointNotFoundException)
            {
                throw new StreamingWebException("Could not connect to backend streaming server.");
            }
        }

        public StreamingSettings Configuration
        {
            get
            {
                try
                {
                    return svc.GetConfiguration();
                }
                catch (FaultException<StreamingFault> e)
                {
                    throw new StreamingWebException(e.Detail.Error);
                }
                catch (EndpointNotFoundException)
                {
                    throw new StreamingWebException("Could not connect to backend streaming server.");
                }
            }
            set
            {
                try
                {
                    svc.SetConfiguration(value);
                }
                catch (FaultException<StreamingFault> e)
                {
                    throw new StreamingWebException(e.Detail.Error);
                }
                catch (EndpointNotFoundException)
                {
                    throw new StreamingWebException("Could not connect to backend streaming server.");
                }
            }
        }

    }
}