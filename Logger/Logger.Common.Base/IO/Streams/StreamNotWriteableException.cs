using System;
using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.IO.Streams
{
    public class StreamNotWriteableException : NotSupportedException
    {
        #region Instance Constructor/Destructor

        public StreamNotWriteableException ()
                : base(Resources.StreamNotWriteableException)
        {
        }

        public StreamNotWriteableException (Exception innerException)
                : base(Resources.StreamNotWriteableException, innerException)
        {
        }

        public StreamNotWriteableException (string message, Exception innerException)
                : base(message, innerException)
        {
        }

        protected StreamNotWriteableException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
