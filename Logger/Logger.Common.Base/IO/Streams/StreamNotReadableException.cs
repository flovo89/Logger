using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Common.Base.IO.Streams
{
    public class StreamNotReadableException : NotSupportedException
    {
        #region Instance Constructor/Destructor

        public StreamNotReadableException()
                : base(Properties.Resources.StreamNotReadableException)
        {
        }

        public StreamNotReadableException(Exception innerException)
                : base(Properties.Resources.StreamNotReadableException, innerException)
        {
        }

        public StreamNotReadableException(string message, Exception innerException)
                : base(message, innerException)
        {
        }

        protected StreamNotReadableException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
