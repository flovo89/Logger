using System;
using System.Runtime.Serialization;




namespace Logger.Common.IO.Streams
{
    public class StreamNotWriteableException : NotSupportedException
    {
        #region Instance Constructor/Destructor

        public StreamNotWriteableException ()
                : base(Properties.Resources.StreamNotWriteableException)
        {
        }

        public StreamNotWriteableException (Exception innerException)
                : base(Properties.Resources.StreamNotWriteableException, innerException)
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
