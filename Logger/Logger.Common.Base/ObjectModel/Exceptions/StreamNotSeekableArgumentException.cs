using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
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
