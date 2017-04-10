using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class StreamNotSeekableArgumentException : InvalidStreamArgumentException
    {
        #region Instance Constructor/Destructor

        public StreamNotSeekableArgumentException (string paramName)
                : base(Properties.Resources.StreamNotSeekableArgumentException, paramName)
        {
        }

        protected StreamNotSeekableArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
