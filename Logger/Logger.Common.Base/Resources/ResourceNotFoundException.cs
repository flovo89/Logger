using System;
using System.Globalization;
using System.Runtime.Serialization;




namespace Logger.Common.Resources
{
    public class ResourceNotFoundException : Exception
    {
        #region Instance Constructor/Destructor

        public ResourceNotFoundException (string resourceName)
                : base(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ResourceNotFoundException, resourceName))
        {
        }

        public ResourceNotFoundException (string resourceName, Exception innerException)
                : base(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ResourceNotFoundException, resourceName), innerException)
        {
        }

        protected ResourceNotFoundException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
