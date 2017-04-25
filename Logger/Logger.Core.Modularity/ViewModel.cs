using System;
using System.ComponentModel.Composition;
using System.Globalization;

using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Messaging;
using Logger.Core.Interfaces.Resources;
using Logger.Core.Interfaces.Session;
using Logger.Core.Interfaces.Settings;

using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;




namespace Logger.Core
{
    [InheritedExport (typeof(IViewModel))]
    [InheritedExport (typeof(ISessionCultureAware))]
    [InheritedExport (typeof(ISessionShutdownAware))]
    [CLSCompliant (false)]
    public abstract class ViewModel : BindableBase,
            IPartImportsSatisfiedNotification,
            IViewModel,
            ISessionCultureAware,
            ISessionShutdownAware
    {
        #region Instance Constructor/Destructor

        [ImportingConstructor]
        protected ViewModel ()
        {
            this.IsInitializedInternal = false;
        }

        #endregion




        #region Instance Properties/Indexer

        private bool IsInitializedInternal { get; set; }

        [Import (typeof(ILogManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(IMessageManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IMessageManager> MessageManager { get; private set; }

        [Import (typeof(IModuleManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IModuleManager> ModuleManager { get; private set; }

        [Import (typeof(IResourceManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IResourceManager> ResourceManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISessionManager> SessionManager { get; private set; }

        [Import (typeof(ISettingManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISettingManager> SettingManager { get; private set; }

        [Import (typeof(IShellManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IShellManager> ShellManager { get; private set; }

        [Import (typeof(ISplashScreenManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISplashScreenManager> SplashScreenManager { get; private set; }

        [Import (typeof(IViewManager), AllowDefault = false, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IViewManager> ViewManager { get; private set; }

        #endregion




        #region Virtuals

        protected virtual string Name => this.GetType().Name;

        protected virtual void ConfirmNavigationRequest (NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        protected virtual void EndBackup ()
        {
        }

        protected virtual void EndRestore ()
        {
        }

        protected virtual void Initialize ()
        {
            this.LogManager.Value.Log(typeof(ViewModel).Name, LogLevel.Debug, "Initializing view model: {0}", this.GetType().Name);
        }

        protected virtual bool IsNavigationTarget (NavigationContext navigationContext)
        {
            return true;
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

        protected virtual void OnNavigatedFrom (NavigationContext navigationContext)
        {
            this.LogManager.Value.Log(typeof(ViewModel).Name, LogLevel.Debug, "Navigating away from view model: {0}", this.GetType().Name);

            this.OnViewDeactivated();
        }

        protected virtual void OnNavigatedTo (NavigationContext navigationContext)
        {
            this.LogManager.Value.Log(typeof(ViewModel).Name, LogLevel.Debug, "Navigating to view model: {0}", this.GetType().Name);

            this.OnViewActivated();
        }

        protected virtual void OnResourceChanged (string key, object value)
        {
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

        protected virtual void OnViewActivated ()
        {
        }

        protected virtual void OnViewDeactivated ()
        {
        }

        #endregion




        #region Interface: IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied ()
        {
            this.OnImportsSatisfied();
        }

        #endregion




        #region Interface: ISessionCultureAware

        void ISessionCultureAware.OnFormattingCultureChanged (CultureInfo formattingCulture)
        {
            this.OnFormattingCultureChanged(formattingCulture);
        }

        void ISessionCultureAware.OnUiCultureChanged (CultureInfo uiCulture)
        {
            this.OnUiCultureChanged(uiCulture);
        }

        #endregion




        #region Interface: ISessionShutdownAware

        void ISessionShutdownAware.OnShutdown (int exitCode)
        {
            this.OnShutdown(exitCode);
        }

        #endregion




        #region Interface: IViewModel

        string IViewModel.Name => this.Name;

        void IConfirmNavigationRequest.ConfirmNavigationRequest (NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            this.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        void IViewModel.Initialize ()
        {
            if (!this.IsInitializedInternal)
            {
                this.IsInitializedInternal = true;
                this.Initialize();
            }
        }

        bool INavigationAware.IsNavigationTarget (NavigationContext navigationContext)
        {
            return this.IsNavigationTarget(navigationContext);
        }

        void INavigationAware.OnNavigatedFrom (NavigationContext navigationContext)
        {
            this.OnNavigatedFrom(navigationContext);
        }

        void INavigationAware.OnNavigatedTo (NavigationContext navigationContext)
        {
            this.OnNavigatedTo(navigationContext);
        }

        #endregion
    }
}
