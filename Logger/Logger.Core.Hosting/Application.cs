using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

using Logger.Common.ObjectModel;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Session;
using Logger.Core.Interfaces.Settings;
using Logger.Core.Resources;

using Microsoft.Practices.Prism.Modularity;
using Microsoft.Win32;




namespace Logger.Core
{
    public class Application : System.Windows.Application,
            IDisposable,
            ISynchronizable,
            IPartImportsSatisfiedNotification,
            ISessionCultureAware,
            ISessionShutdownAware,
            IResourceAware
    {
        #region Static Properties/Indexer

        public new static Application Current
        {
            get
            {
                return System.Windows.Application.Current as Application;
            }
        }

        #endregion




        #region Instance Constructor/Destructor

        public Application ()
        {
            this.SyncRoot = new object();
        }

        #endregion




        #region Instance Properties/Indexer

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(IModuleManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IModuleManager> ModuleManager { get; private set; }

        [Import (typeof(IResourceManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IResourceManager> ResourceManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISessionManager> SessionManager { get; private set; }

        [Import (typeof(ISettingManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISettingManager> SettingManager { get; private set; }

        [Import (typeof(IShellManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IShellManager> ShellManager { get; private set; }

        [Import (typeof(ISplashScreenManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISplashScreenManager> SplashScreenManager { get; private set; }

        [Import (typeof(IViewManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IViewManager> ViewManager { get; private set; }

        #endregion




        #region Instance Events

        public event PowerModeChangedEventHandler PowerModeChanged;

        public event SessionEndedEventHandler SessionEnded;

        public event SessionSwitchEventHandler SessionSwitch;

        #endregion




        #region Virtuals

        protected virtual void Dispose ()
        {
        }

        protected virtual void OnFormattingCultureChanged (CultureInfo formattingCulture)
        {
        }

        protected virtual void OnImportsSatisfied ()
        {
        }

        protected virtual void OnLicenseChanged (string key, string stringValue)
        {
        }

        protected virtual void OnPowerModeChanged (PowerModeChangedEventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Information, "Changing power mode: {0}", e.Mode);
            }

            PowerModeChangedEventHandler handler = this.PowerModeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnResourceChanged (string key, object value)
        {
        }

        protected virtual void OnSessionEnded (SessionEndedEventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Information, "Post-Ending session: {0}", e.Reason);
            }

            SessionEndedEventHandler handler = this.SessionEnded;
            if (handler != null)
            {
                handler(this, e);
            }

            Lazy<ISessionManager> sessionManager = this.SessionManager;
            if (sessionManager != null)
            {
                sessionManager.Value.Shutdown(SessionExitCodes.SessionEnding);
            }
        }

        protected virtual void OnSessionSwitch (SessionSwitchEventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Information, "Switching session: {0}", e.Reason);
            }

            SessionSwitchEventHandler handler = this.SessionSwitch;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSettingChanged (string key, string stringValue)
        {
        }

        protected virtual void OnShutdown (int exitCode)
        {
        }

        protected virtual void OnUiCultureChanged (CultureInfo uiCulture)
        {
        }

        #endregion




        #region Overrides

        protected override void OnActivated (EventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Activating application");
            }

            base.OnActivated(e);
        }

        protected override void OnDeactivated (EventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Deactivating application");
            }

            base.OnDeactivated(e);
        }

        protected override void OnExit (ExitEventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Information, "Exiting application: {0}", e.ApplicationExitCode);
            }

            base.OnExit(e);
        }

        protected sealed override void OnFragmentNavigation (FragmentNavigationEventArgs e)
        {
            base.OnFragmentNavigation(e);
        }

        protected sealed override void OnLoadCompleted (NavigationEventArgs e)
        {
            base.OnLoadCompleted(e);
        }

        protected sealed override void OnNavigated (NavigationEventArgs e)
        {
            base.OnNavigated(e);
        }

        protected sealed override void OnNavigating (NavigatingCancelEventArgs e)
        {
            base.OnNavigating(e);
        }

        protected sealed override void OnNavigationFailed (NavigationFailedEventArgs e)
        {
            base.OnNavigationFailed(e);
        }

        protected sealed override void OnNavigationProgress (NavigationProgressEventArgs e)
        {
            base.OnNavigationProgress(e);
        }

        protected sealed override void OnNavigationStopped (NavigationEventArgs e)
        {
            base.OnNavigationStopped(e);
        }

        protected override void OnSessionEnding (SessionEndingCancelEventArgs e)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Information, "Pre-Ending session: {0}", e.ReasonSessionEnding);
            }

            base.OnSessionEnding(e);
        }

        protected override void OnStartup (StartupEventArgs e)
        {
            base.OnStartup(e);

            this.SessionManager.Value.Container.ComposeExportedValue<ISessionShutdownAware>(this);
            this.SessionManager.Value.Container.ComposeExportedValue<ISessionCultureAware>(this);
            this.SessionManager.Value.Container.ComposeExportedValue<IResourceAware>(this);

            SystemEvents.SessionEnded += (sender, e2) => this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<SessionEndedEventArgs>(this.OnSessionEnded), e2);
            SystemEvents.SessionSwitch += (sender, e2) => this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<SessionSwitchEventArgs>(this.OnSessionSwitch), e2);
            SystemEvents.PowerModeChanged += (sender, e2) => this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<PowerModeChangedEventArgs>(this.OnPowerModeChanged), e2);
        }

        #endregion




        #region Interface: IDisposable

        void IDisposable.Dispose ()
        {
            lock (this.SyncRoot)
            {
                this.Dispose();
            }
        }

        #endregion




        #region Interface: IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied ()
        {
            lock (this.SyncRoot)
            {
                this.OnImportsSatisfied();
            }
        }

        #endregion




        #region Interface: IResourceAware

        void IResourceAware.OnResourceChanged (string key, object value)
        {
            lock (this.SyncRoot)
            {
                this.OnResourceChanged(key, value);
            }
        }

        #endregion




        #region Interface: ISessionCultureAware

        void ISessionCultureAware.OnFormattingCultureChanged (CultureInfo formattingCulture)
        {
            lock (this.SyncRoot)
            {
                this.OnFormattingCultureChanged(formattingCulture);
            }
        }

        void ISessionCultureAware.OnUiCultureChanged (CultureInfo uiCulture)
        {
            lock (this.SyncRoot)
            {
                this.OnUiCultureChanged(uiCulture);
            }
        }

        #endregion




        #region Interface: ISessionShutdownAware

        void ISessionShutdownAware.OnShutdown (int exitCode)
        {
            lock (this.SyncRoot)
            {
                this.OnShutdown(exitCode);
            }
        }

        #endregion




        #region Interface: ISynchronizable

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        #endregion
    }
}
