using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Termors.Services.Tv.MPStreamingInterface
{
    [DataContract]
    public class StreamingFault
    {
        private string m_sError;

        public StreamingFault()
        {
        }

        public StreamingFault(string errorMessage)
        {
            Error = errorMessage;
        }

        [DataMember]
        public string Error
        {
            get
            {
                return m_sError;
            }
            set
            {
                m_sError = value;
            }
        }
    }
}