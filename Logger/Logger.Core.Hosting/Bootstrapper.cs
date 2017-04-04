using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using Logger.Common.Base.DataTypes;
using Logger.Core.Hosting.Properties;
using Logger.Core.Interfaces.Logging;

using Prism.Mef;




namespace Logger.Core.Hosting
{
    public class Bootstrapper : MefBootstrapper
    {
        #region Instance Constructor/Destructor

        public Bootstrapper ()
        {
            this.IsRunning = false;
            this.IsShuttingDown = false;
        }

        #endregion




        #region Instance Properties/Indexer

        public string ApplicationName { get; private set; }

        private bool IsRunning { get; set; }

        private bool IsShuttingDown { get; }

        #endregion




        #region Instance Methods

        public new int Run (bool runWithDefaultConfiguration)
        {
            try
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.Bootstrapper_AnotherInstanceIsAlreadyRunning, this.GetType().Name));
                }

                this.IsRunning = true;

                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    try
                    {
                        Exception exception = e.ExceptionObject as Exception;
                        if (exception == null)
                        {
                            return;
                        }

                        string exceptionMessage = exception.ToDetailedString(' ');
                        string exceptionCaption = this.ApplicationName ?? exception.Source;

                        try
                        {
                            Debugger.Log((int)LogLevel.Error, exceptionCaption, exceptionMessage);
                        }
                        catch
                        {
                        }

                        try
                        {
                            Lazy<ILogManager> logger = this.LogManager;
                            if (logger != null)
                            {
                                if (logger.IsValueCreated)
                                {
                                    logger.Value.Log(this.GetType().Name, LogLevel.Error, "-------------------- CRASH --------------------");
                                    logger.Value.Log(this.GetType().Name, LogLevel.Error, exceptionMessage);
                                }
                            }
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (exception is ThreadAbortException && this.IsShuttingDown)
                            {
                                return;
                            }
                        }
                        catch
                        {
                        }

                        if (!Debugger.IsAttached)
                        {
                            try
                            {
                                this.Cleanup();
                            }
                            catch
                            {
                            }

                            Environment.FailFast(exceptionMessage, exception);
                        }
                    }
                    catch
                    {
                    }
                };
            }
            catch
            {
            }

            return Environment.ExitCode;
        }

        public new int Run ()
        {
            return this.Run(true);
        }

        #endregion




        #region Virtuals

        protected virtual void Cleanup ()
        {
            if (this.Application != null)
            {
                if (this.Application is IDisposable)
                {
                    ( (IDisposable)this.Application ).Dispose();
                }
            }

            if (this.Container != null)
            {
                this.Container.Dispose();
            }

            if (this.Catalog != null)
            {
                this.Catalog.Dispose();
            }

            if (this.ApplicationIcon != null)
            {
                this.ApplicationIcon.Dispose();
            }

            if (( this.SessionMutex != null ) && this.SessionMutexCreated)
            {
                this.SessionMutex.Close();
                this.SessionMutex.Dispose();
                this.SessionMutex = null;
            }
        }

        #endregion
    }
}
