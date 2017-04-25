using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
{
    public class StreamNotWriteableArgumentException : InvalidStreamArgumentException
    {
        #region Instance Constructor/Destructor

        public StreamNotWriteableArgumentException (string paramName)
                : base(Properties.Resources.StreamNotWriteableArgumentException, paramName)
        {
        }

        protected StreamNotWriteableArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
