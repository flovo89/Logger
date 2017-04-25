using System;

using Microsoft.Practices.Prism.Logging;




namespace Logger.Common.Prism
{
    public sealed class DelegateLogger : ILoggerFacade
    {
        #region Instance Constructor/Destructor

        public DelegateLogger (Action<string, Category, Priority> loggingDelegate)
        {
            if (loggingDelegate == null)
            {
                throw new ArgumentNullException(nameof(loggingDelegate));
            }

            this.LoggingDelegate = loggingDelegate;
        }

        #endregion




        #region Instance Properties/Indexer

        private Action<string, Category, Priority> LoggingDelegate { get; }

        #endregion




        #region Interface: ILoggerFacade

        public void Log (string message, Category category, Priority priority)
        {
            this.LoggingDelegate(message, category, priority);
        }

        #endregion
    }
}
