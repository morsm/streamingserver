using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Termors.Services.Tv.MPStreamingService
{
    [Serializable]
    public class StreamingException : Exception
    {
        public StreamingException(string message)
            : base(message)
        {
        }
    }
}
