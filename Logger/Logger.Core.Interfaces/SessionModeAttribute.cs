using System;

using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;




namespace Logger.Core.Interfaces
{
    [AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class SessionModeAttribute : Attribute
    {
        #region Instance Constructor/Destructor

        public SessionModeAttribute (string sessionMode)
        {
            if (sessionMode == null)
            {
                throw new ArgumentNullException(nameof(sessionMode));
            }

            if (sessionMode.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(sessionMode));
            }

            this.SessionMode = sessionMode;
        }

        #endregion




        #region Instance Properties/Indexer

        public string SessionMode { get; private set; }

        #endregion
    }
}
