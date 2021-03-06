﻿using System;
using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
{
    public class EmptyStringArgumentException : ArgumentException
    {
        #region Instance Constructor/Destructor

        public EmptyStringArgumentException (string paramName)
                : base(Properties.Resources.EmptyStringArgumentException, paramName)
        {
        }

        protected EmptyStringArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
