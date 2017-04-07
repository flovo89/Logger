using System;
using System.Globalization;
using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.Conversion
{
    public class ConversionNotPossibleException : NotSupportedException
    {
        #region Instance Constructor/Destructor

        public ConversionNotPossibleException (Type sourceType, Type targetType)
                : base(string.Format(CultureInfo.InvariantCulture, Resources.ConversionNotPossibleException, sourceType.FullName, targetType.FullName))
        {
        }

        public ConversionNotPossibleException (Type sourceType, Type targetType, Exception innerException)
                : base(string.Format(CultureInfo.InvariantCulture, Resources.ConversionNotPossibleException, sourceType.FullName, targetType.FullName), innerException)
        {
        }

        public ConversionNotPossibleException (string message)
                : base(message)
        {
        }

        public ConversionNotPossibleException (string message, Exception innerException)
                : base(message, innerException)
        {
        }

        protected ConversionNotPossibleException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
