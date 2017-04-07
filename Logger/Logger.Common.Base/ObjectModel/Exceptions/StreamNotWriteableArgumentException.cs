using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class StreamNotWriteableArgumentException : InvalidStreamArgumentException
    {
        #region Instance Constructor/Destructor

        public StreamNotWriteableArgumentException (string paramName)
                : base(Resources.StreamNotWriteableArgumentException, paramName)
        {
        }

        protected StreamNotWriteableArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
