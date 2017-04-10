using System.Runtime.Serialization;

using Logger.Common.Base.Properties;




namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class PathNotDirectoryArgumentException : InvalidPathArgumentException
    {
        #region Instance Constructor/Destructor

        public PathNotDirectoryArgumentException (string paramName)
                : base(Properties.Resources.PathNotDirectoryArgumentException, paramName)
        {
        }

        protected PathNotDirectoryArgumentException (SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
