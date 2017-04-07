using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;

using Logger.Common.Base.DataTypes;
using Logger.Common.Base.Reflection;
using Logger.Core.Hosting.Properties;
using Logger.Core.Interfaces.Logging;

using Prism.Mef;




namespace Logger.Core.Hosting
{
    public class Bootstrapper : MefBootstrapper
    {
        #region Constants

        private const string DefaultPrimaryThreadName = "PrimaryThread";

        private const string DefaultSplashScreenThreadName = "SplashScreenThread ({0})";

        private const ThreadPriority DefaultSplashScreenThreadPriority = ThreadPriority.Highest;

        #endregion




        #region Instance Constructor/Destructor

        public Bootstrapper ()
        {
            this.IsRunning = false;
            this.IsShuttingDown = false;
        }

        #endregion




        #region Instance Properties/Indexer

        public Application Application { get; private set; }

        public Assembly ApplicationAssembly { get; private set; }

        public string ApplicationCompany { get; private set; }

        public string ApplicationCopyright { get; private set; }

        public Icon ApplicationIcon { get; private set; }

        public string ApplicationName { get; private set; }

        public AggregateCatalog Catalog { get; private set; }

        public new CompositionContainer Container
        {
            get
            {
                return base.Container;
            }
        }

        public CultureInfo InitialFormattingCulture { get; private set; }

        public CultureInfo InitialUiCulture { get; private set; }

        public Thread PrimaryThread { get; private set; }

        public Process Process { get; private set; }

        public Guid SessionId { get; private set; }

        public string SplashScreenThreadName { get; private set; }

        public ThreadPriority SplashScreenThreadPriority { get; private set; }

        private bool IsRunning { get; set; }

        private bool IsShuttingDown { get; }

        private Mutex SessionMutex { get; set; }

        private bool SessionMutexCreated { get; set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ILogManager> LogManager { get; private set; }

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

                this.InitialUiCulture = CultureInfo.CurrentUICulture;
                this.InitialFormattingCulture = CultureInfo.CurrentCulture;

                this.SessionId = Guid.NewGuid();

                this.Process = Process.GetCurrentProcess();
                this.Process.PriorityClass = this.GetProcessPriority();

                this.PrimaryThread = Thread.CurrentThread;
                this.PrimaryThread.Name = this.GetPrimaryThreadName() ?? Bootstrapper.DefaultPrimaryThreadName;
                this.PrimaryThread.Priority = this.GetPrimaryThreadPriority();

                this.SplashScreenThreadName = this.GetSplashScreenThreadName() ?? Bootstrapper.DefaultSplashScreenThreadName;
                this.SplashScreenThreadPriority = this.GetSplashScreenThreadPriority();

                this.ApplicationAssembly = this.GetApplicationAssembly();
                if (this.ApplicationAssembly == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationAssemblyIsRequired);
                }

                this.ApplicationName = this.GetApplicationName();
                if (this.ApplicationName == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationNameIsRequired);
                }
                if (this.ApplicationName.IsEmpty())
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationNameIsRequired);
                }

                this.ApplicationCompany = this.GetApplicationCompany();
                if (this.ApplicationCompany == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationCompanyIsRequired);
                }
                if (this.ApplicationCompany.IsEmpty())
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationCompanyIsRequired);
                }

                this.ApplicationCopyright = this.GetApplicationCopyright();
                if (this.ApplicationCopyright == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationCopyrightIsRequired);
                }
                if (this.ApplicationCopyright.IsEmpty())
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationCopyrightIsRequired);
                }

                this.ApplicationIcon = this.GetApplicationIcon();
                if (this.ApplicationIcon == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationIconIsRequired);
                }
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

        protected virtual Assembly GetApplicationAssembly ()
        {
            return Assembly.GetEntryAssembly();
        }

        protected virtual string GetApplicationCompany ()
        {
            return this.ApplicationAssembly.GetCompany();
        }

        protected virtual string GetApplicationCopyright ()
        {
            return this.ApplicationAssembly.GetCopyright();
        }

        protected virtual Icon GetApplicationIcon ()
        {
            return this.ApplicationAssembly.GetDefaultIcon();
        }

        protected virtual string GetApplicationName ()
        {
            return this.ApplicationAssembly.GetProductName();
        }

        protected virtual string GetPrimaryThreadName ()
        {
            return Bootstrapper.DefaultPrimaryThreadName;
        }

        protected virtual ThreadPriority GetPrimaryThreadPriority ()
        {
            return Thread.CurrentThread.Priority;
        }

        protected virtual ProcessPriorityClass GetProcessPriority ()
        {
            return Process.GetCurrentProcess().PriorityClass;
        }

        protected virtual string GetSplashScreenThreadName ()
        {
            return Bootstrapper.DefaultSplashScreenThreadName;
        }

        protected virtual ThreadPriority GetSplashScreenThreadPriority ()
        {
            return Bootstrapper.DefaultSplashScreenThreadPriority;
        }

        #endregion
    }
}
