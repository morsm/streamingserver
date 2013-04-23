using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamingApp
{
    [Serializable]
    public class StreamingWebException : Exception
    {
        public StreamingWebException(string message)
            : base(message)
        {
        }
    }
}