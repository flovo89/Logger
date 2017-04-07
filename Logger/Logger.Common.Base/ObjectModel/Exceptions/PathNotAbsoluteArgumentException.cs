using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class PathNotAbsoluteArgumentException : InvalidPathArgumentException
    {
        #region Instance Constructor/Destructor

        public PathNotAbsoluteArgumentException (string paramName)
                : base(Resources.PathNotAbsoluteArgumentException, paramName)
        {
        }

        protected PathNotAbsoluteArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
