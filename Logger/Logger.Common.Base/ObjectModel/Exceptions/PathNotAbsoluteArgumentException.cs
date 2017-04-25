using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
{
    public class PathNotAbsoluteArgumentException : InvalidPathArgumentException
    {
        #region Instance Constructor/Destructor

        public PathNotAbsoluteArgumentException (string paramName)
                : base(Properties.Resources.PathNotAbsoluteArgumentException, paramName)
        {
        }

        protected PathNotAbsoluteArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
