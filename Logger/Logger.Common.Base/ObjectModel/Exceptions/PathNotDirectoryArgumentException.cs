using System.Runtime.Serialization;




namespace Logger.Common.ObjectModel.Exceptions
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
