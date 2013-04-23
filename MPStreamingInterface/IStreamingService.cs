using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [ServiceContract]
    public interface IStreamingService
    {
        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        ChannelList GetChannelList();

        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        StreamingResult StartStreaming(int channelId, bool record, bool copyToFtp);

        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        void StopStreaming(StreamingResult stresult);

        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        StreamingResult[] GetStreamingStatus();

        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        StreamingSettings GetConfiguration();

        [OperationContract]
        [FaultContract(typeof(StreamingFault))]
        void SetConfiguration(StreamingSettings settings);
    }
}
