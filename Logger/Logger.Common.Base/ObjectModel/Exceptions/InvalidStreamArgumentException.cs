﻿using System;
using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
{
    [Serializable]
    public class InvalidStreamArgumentException : ArgumentException
    {
        #region Instance Constructor/Destructor

        public InvalidStreamArgumentException (string paramName)
                : base(Properties.Resources.InvalidStreamArgumentException, paramName)
        {
        }

        public InvalidStreamArgumentException (string message, string paramName)
                : base(message, paramName)
        {
        }

        protected InvalidStreamArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
