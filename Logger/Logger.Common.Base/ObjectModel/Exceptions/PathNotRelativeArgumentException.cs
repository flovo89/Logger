using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
{
    public class PathNotRelativeArgumentException : InvalidPathArgumentException
    {
        #region Instance Constructor/Destructor

        public PathNotRelativeArgumentException (string paramName)
                : base(Properties.Resources.PathNotRelativeArgumentException, paramName)
        {
        }

        protected PathNotRelativeArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
