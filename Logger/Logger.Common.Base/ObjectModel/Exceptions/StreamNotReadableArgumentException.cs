using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class StreamNotReadableArgumentException : InvalidStreamArgumentException
    {
        #region Instance Constructor/Destructor

        public StreamNotReadableArgumentException (string paramName)
                : base(Properties.Resources.StreamNotReadableArgumentException, paramName)
        {
        }

        protected StreamNotReadableArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
