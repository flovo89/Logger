using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using Logger.Common.IO.Files;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Session;




namespace Logger.Core.Session
{
    [Export (typeof(ISessionManager))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class SessionManager : ISessionManager
    {
        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public SessionManager ()
        {
            this.SyncRoot = new object();

            this.IsShuttingDown = false;
        }

        #endregion




        #region Instance Properties/Indexer

        [Import (typeof(Bootstrapper), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Bootstrapper Bootstrapper { get; private set; }

        [ImportMany (typeof(ISessionCultureAware), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<ISessionCultureAware>> CultureAwares { get; private set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ILogManager> LogManager { get; private set; }

        [ImportMany (typeof(ISessionShutdownAware), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<ISessionShutdownAware>> ShutdownAwares { get; private set; }

        private bool IsShuttingDown { get; set; }

        #endregion




        #region Interface: ISessionManager

        public Application Application
        {
            get
            {
                return this.Bootstrapper.Application;
            }
        }

        public string ApplicationCompany
        {
            get
            {
                return this.Bootstrapper.ApplicationCompany;
            }
        }

        public string ApplicationCopyright
        {
            get
            {
                return this.Bootstrapper.ApplicationCopyright;
            }
        }

        public Icon ApplicationIcon
        {
            get
            {
                return this.Bootstrapper.ApplicationIcon;
            }
        }

        public Guid ApplicationId
        {
            get
            {
                return this.Bootstrapper.ApplicationId;
            }
        }

        public string ApplicationName
        {
            get
            {
                return this.Bootstrapper.ApplicationName;
            }
        }

        public Version ApplicationVersion
        {
            get
            {
                return this.Bootstrapper.ApplicationVersion;
            }
        }

        public AggregateCatalog Catalog
        {
            get
            {
                return this.Bootstrapper.Catalog;
            }
        }

        public CompositionContainer Container
        {
            get
            {
                return this.Bootstrapper.Container;
            }
        }

        public DirectoryPath DataFolder
        {
            get
            {
                return this.Bootstrapper.DataDirectory;
            }
        }

        public bool DebugMode
        {
            get
            {
                return this.Bootstrapper.DebugMode;
            }
        }

        public Dispatcher Dispatcher
        {
            get
            {
                return this.Application.Dispatcher;
            }
        }

        public DirectoryPath ExecutableFolder
        {
            get
            {
                return this.Bootstrapper.ApplicationDirectory;
            }
        }

        public IDictionary<string, IList<string>> InitialSettings
        {
            get
            {
                return this.Bootstrapper.InitialSettings;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public Guid SessionId
        {
            get
            {
                return this.Bootstrapper.SessionId;
            }
        }

        public IEnumerable<string> SessionMode
        {
            get
            {
                return this.Bootstrapper.SessionMode;
            }
        }

        public string SplashScreenThreadName
        {
            get
            {
                return this.Bootstrapper.SplashScreenThreadName;
            }
        }

        public ThreadPriority SplashScreenThreadPriority
        {
            get
            {
                return this.Bootstrapper.SplashScreenThreadPriority;
            }
        }

        public CultureInfo StartupFormattingCulture
        {
            get
            {
                return this.Bootstrapper.InitialFormattingCulture;
            }
        }

        public DateTime StartupTimestamp
        {
            get
            {
                return this.Bootstrapper.Process.StartTime;
            }
        }

        public CultureInfo StartupUiCulture
        {
            get
            {
                return this.Bootstrapper.InitialUiCulture;
            }
        }

        public object SyncRoot { get; }

        public DirectoryPath TemporaryFolder
        {
            get
            {
                return this.Bootstrapper.TemporaryDirectory;
            }
        }

        public bool UserInteractive
        {
            get
            {
                return this.Bootstrapper.UserInteractive;
            }
        }

        public void SetFormattingCulture (CultureInfo formattingCulture)
        {
            lock (this.SyncRoot)
            {
                formattingCulture = formattingCulture ?? this.StartupFormattingCulture;

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Notifying new formatting culture setting: {0}", formattingCulture);

                Lazy<ISessionCultureAware>[] sessionCultureAwares = this.CultureAwares.ToArray();
                foreach (Lazy<ISessionCultureAware> cultureAware in sessionCultureAwares)
                {
                    cultureAware.Value.OnFormattingCultureChanged(formattingCulture);
                }
            }
        }

        public void SetUiCulture (CultureInfo uiCulture)
        {
            lock (this.SyncRoot)
            {
                uiCulture = uiCulture ?? this.StartupUiCulture;

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Notifying new UI culture setting: {0}", uiCulture);

                Lazy<ISessionCultureAware>[] sessionCultureAwares = this.CultureAwares.ToArray();
                foreach (Lazy<ISessionCultureAware> cultureAware in sessionCultureAwares)
                {
                    cultureAware.Value.OnUiCultureChanged(uiCulture);
                }
            }
        }

        public void Shutdown (int exitCode)
        {
            lock (this.SyncRoot)
            {
                if (this.IsShuttingDown)
                {
                    return;
                }

                this.IsShuttingDown = true;

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Notifying shutdown: {0}", exitCode);

                Lazy<ISessionShutdownAware>[] sessionShutdownAwares = this.ShutdownAwares.ToArray();
                foreach (Lazy<ISessionShutdownAware> shutdownAware in sessionShutdownAwares)
                {
                    shutdownAware.Value.OnShutdown(exitCode);
                }
            }
        }

        #endregion
    }
}
