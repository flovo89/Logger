using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Common.Base.ObjectModel.Exceptions
{
    public class PathNotRelativeArgumentException : InvalidPathArgumentException
    {
        #region Instance Constructor/Destructor

        public PathNotRelativeArgumentException(string paramName)
                : base(Properties.Resources.PathNotRelativeArgumentException, paramName)
        {
        }

        protected PathNotRelativeArgumentException(SerializationInfo info, StreamingContext context)
                : base(info, context)
        {
        }

        #endregion
    }
}
