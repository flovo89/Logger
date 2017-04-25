using System;
using System.Runtime.Serialization;




namespace Logger.Common.IO.Streams
{
    public class StreamNotReadableException : NotSupportedException
    {
        #region Instance Constructor/Destructor

        public StreamNotReadableException ()
                : base(Properties.Resources.StreamNotReadableException)
        {
        }

        public StreamNotReadableException (Exception innerException)
                : base(Properties.Resources.StreamNotReadableException, innerException)
        {
        }

        public StreamNotReadableException (string message, Exception innerException)
                : base(message, innerException)
        {
        }

        protected StreamNotReadableException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
