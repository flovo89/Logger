using System;
using System.Runtime.Serialization;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    [Serializable]
    public class InvalidPathArgumentException : ArgumentException
    {
        #region Instance Constructor/Destructor

        public InvalidPathArgumentException (string message, string paramName)
                : base(message, paramName)
        {
        }

        public InvalidPathArgumentException (string paramName)
                : base(string.Empty, paramName)
        {
        }

        protected InvalidPathArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
