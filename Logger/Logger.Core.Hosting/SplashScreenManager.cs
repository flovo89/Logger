using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

using Logger.Common.Collections.Generic;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel.Exceptions;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Session;




namespace Logger.Core
{
    [Export (typeof(ISplashScreenManager))]
    [Export (typeof(ISessionCultureAware))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class SplashScreenManager : ISplashScreenManager,
            ISessionCultureAware,
            IPartImportsSatisfiedNotification,
            IDisposable
    {
        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public SplashScreenManager ()
        {
            this.SyncRoot = new object();
        }

        ~SplashScreenManager ()
        {
            this.Dispose();
        }

        #endregion




        #region Instance Properties/Indexer

        public string CachedFooterText { get; private set; }

        public string CachedLicenseInformation { get; private set; }

        public string CachedStatusText { get; private set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ISessionManager> SessionManager { get; private set; }

        [ImportMany (typeof(ISplashScreen), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<ISplashScreen>> SplashScreens { get; private set; }

        private Dictionary<ISplashScreen, Thread> SplashScreenThreads { get; set; }

        #endregion




        #region Instance Methods

        private void CreateSplashScreenThread (Lazy<ISplashScreen> splashScreenInitializer, out ISplashScreen splashScreen, out Thread thread)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Creating new splash screen thread");
            }

            using (ManualResetEvent splashScreenCreationSynchronization = new ManualResetEvent(false))
            {
                ISplashScreen tempSplashScreen = null;
                Thread tempThread = null;

                tempThread = new Thread(() =>
                {
                    tempSplashScreen = splashScreenInitializer.Value;
                    Thread.CurrentThread.Name = string.Format(CultureInfo.InvariantCulture, this.SessionManager.Value.SplashScreenThreadName, splashScreenInitializer.Value.GetType().Name);
                    tempSplashScreen.Initialize();
                    // ReSharper disable once AccessToDisposedClosure
                    splashScreenCreationSynchronization.Set();
                    Dispatcher.Run();
                });

                tempThread.IsBackground = true;
                tempThread.Priority = this.SessionManager.Value.SplashScreenThreadPriority;
                tempThread.SetApartmentState(Thread.CurrentThread.GetApartmentState());

                tempThread.Start();
                splashScreenCreationSynchronization.WaitOne();

                splashScreen = tempSplashScreen;
                thread = tempThread;
            }

            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Created new splash screen thread: {0} -> [{1}]", splashScreen.GetType().Name, thread.ManagedThreadId);
            }
        }

        private void DestroySplashScreenThread (ISplashScreen splashScreen, Thread thread)
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Destroying splash screen thread: {0} -> {1}", splashScreen.GetType().Name, thread.ManagedThreadId);
            }

            splashScreen.Hide();

            Dispatcher dispatcher = Dispatcher.FromThread(thread);
            if (dispatcher != null)
            {
                dispatcher.InvokeShutdown();
            }

            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Destroyed splash screen thread");
            }
        }

        private void InitializeSplashScreen (ISplashScreen splashScreen)
        {
            if (splashScreen != null)
            {
                splashScreen.Initialize();
            }
        }

        private void UpdateFooterText ()
        {
            Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
            foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
            {
                splashScreen.Value.SetFooter(this.CachedFooterText);
            }
        }

        private void UpdateLicenseInformation ()
        {
            Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
            foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
            {
                splashScreen.Value.SetLicenseInformation(this.CachedLicenseInformation);
            }
        }

        private void UpdateSplashScreens ()
        {
            Lazy<ILogManager> logger = this.LogManager;
            if (logger != null)
            {
                logger.Value.Log(this.GetType().Name, LogLevel.Debug, "Updating and initializing splash screens");
            }

            Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();

            if (this.SplashScreenThreads == null)
            {
                this.SplashScreenThreads = new Dictionary<ISplashScreen, Thread>();
            }

            Dictionary<ISplashScreen, Thread> newThreads = new Dictionary<ISplashScreen, Thread>();
            Dictionary<ISplashScreen, Thread> oldThreads = new Dictionary<ISplashScreen, Thread>();

            foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
            {
                if (splashScreen.IsValueCreated)
                {
                    continue;
                }
                ISplashScreen newSplashScreen = null;
                Thread newThread = null;
                this.CreateSplashScreenThread(splashScreen, out newSplashScreen, out newThread);
                if (( newSplashScreen == null ) || ( newThread == null ))
                {
                    continue;
                }
                newThreads.Add(newSplashScreen, newThread);
            }

            foreach (KeyValuePair<ISplashScreen, Thread> splashScreenThread in this.SplashScreenThreads)
            {
                bool found = false;
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    if (splashScreen.IsValueCreated)
                    {
                        if (splashScreenThread.Key.Equals(splashScreen.Value))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    this.DestroySplashScreenThread(splashScreenThread.Key, splashScreenThread.Value);
                    oldThreads.Add(splashScreenThread.Key, splashScreenThread.Value);
                }
            }

            this.SplashScreenThreads.RemoveRange(oldThreads.Keys);
            this.SplashScreenThreads.AddRange(newThreads);
        }

        private void UpdateStatusText ()
        {
            Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
            foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
            {
                splashScreen.Value.SetStatus(this.CachedStatusText);
            }
        }

        #endregion




        #region Interface: IDisposable

        public void Dispose ()
        {
            if (this.SplashScreenThreads != null)
            {
                foreach (KeyValuePair<ISplashScreen, Thread> splashScreenThread in this.SplashScreenThreads)
                {
                    this.DestroySplashScreenThread(splashScreenThread.Key, splashScreenThread.Value);
                }
                this.SplashScreenThreads.Clear();
            }
        }

        #endregion




        #region Interface: IPartImportsSatisfiedNotification

        public void OnImportsSatisfied ()
        {
            lock (this.SyncRoot)
            {
                this.UpdateSplashScreens();
                this.UpdateFooterText();
                this.UpdateStatusText();
                this.UpdateLicenseInformation();
            }
        }

        #endregion




        #region Interface: ISessionCultureAware

        public void OnFormattingCultureChanged (CultureInfo formattingCulture)
        {
            foreach (KeyValuePair<ISplashScreen, Thread> splashScreenThread in this.SplashScreenThreads)
            {
                Dispatcher dispatcher = Dispatcher.FromThread(splashScreenThread.Value);
                if (dispatcher != null)
                {
                    dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<CultureInfo, ISplashScreen, Thread>((formattingCulture2, splashScreen2, thread2) =>
                    {
                        this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting formatting culture for splash screen: {0} -> [{1}]", splashScreen2.GetType().Name, formattingCulture2);
                        thread2.CurrentCulture = formattingCulture2;
                    }), formattingCulture, splashScreenThread.Key, splashScreenThread.Value);
                }
            }
        }

        public void OnUiCultureChanged (CultureInfo uiCulture)
        {
            foreach (KeyValuePair<ISplashScreen, Thread> splashScreenThread in this.SplashScreenThreads)
            {
                Dispatcher dispatcher = Dispatcher.FromThread(splashScreenThread.Value);
                if (dispatcher != null)
                {
                    dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<CultureInfo, ISplashScreen, Thread>((uiCulture2, splashScreen2, thread2) =>
                    {
                        this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting UI culture for splash screen: {0} -> [{1}]", splashScreen2.GetType().Name, uiCulture2);
                        thread2.CurrentUICulture = uiCulture2;
                    }), uiCulture, splashScreenThread.Key, splashScreenThread.Value);
                }
            }
        }

        #endregion




        #region Interface: ISplashScreenManager

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        public ISplashScreen GetSplashScreen (string splashScreenName)
        {
            if (splashScreenName == null)
            {
                throw new ArgumentNullException(nameof(splashScreenName));
            }

            if (splashScreenName.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(splashScreenName));
            }

            lock (this.SyncRoot)
            {
                ISplashScreen splashScreen = this.SessionManager.Value.Container.GetExportedValue<object>(splashScreenName) as ISplashScreen;
                this.InitializeSplashScreen(splashScreen);
                return splashScreen;
            }
        }

        public ISplashScreen[] GetSplashScreens (string splashScreenName)
        {
            if (splashScreenName == null)
            {
                throw new ArgumentNullException(nameof(splashScreenName));
            }

            if (splashScreenName.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(splashScreenName));
            }

            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.SessionManager.Value.Container.GetExportedValues<object>(splashScreenName).ToArray().OfType<ISplashScreen>().ToArray();
                foreach (ISplashScreen splashScreen in splashScreens)
                {
                    this.InitializeSplashScreen(splashScreen);
                }
                return splashScreens;
            }
        }

        public void Hide (string splashScreenName)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.Hide();
                    }
                }
            }
        }

        public void HideAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.Hide();
                }
            }
        }

        public void MoveAllToBackground ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.MoveToBackground();
                }
            }
        }

        public void MoveAllToForeground ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.MoveToForeground();
                }
            }
        }

        public void MoveAllToPrimaryScreen ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.MoveToPrimaryScreen();
                }
            }
        }

        public void MoveAllToScreen (int screenIndex)
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.MoveToScreen(screenIndex);
                }
            }
        }

        public void MoveAllToScreen (Screen screen)
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.MoveToScreen(screen);
                }
            }
        }

        public void MoveToBackground (string splashScreenName)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.MoveToBackground();
                    }
                }
            }
        }

        public void MoveToForeground (string splashScreenName)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.MoveToForeground();
                    }
                }
            }
        }

        public void MoveToPrimaryScreen (string splashScreenName)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.MoveToPrimaryScreen();
                    }
                }
            }
        }

        public void MoveToScreen (string splashScreenName, int screenIndex)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.MoveToScreen(screenIndex);
                    }
                }
            }
        }

        public void MoveToScreen (string splashScreenName, Screen screen)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.MoveToScreen(screen);
                    }
                }
            }
        }

        public void SetAllFooter (string footerText)
        {
            lock (this.SyncRoot)
            {
                this.CachedFooterText = footerText;
                this.UpdateFooterText();
            }
        }

        public void SetAllHideMouseCursor (bool hideMouseCursor)
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.SetHideMouseCursor(hideMouseCursor);
                }
            }
        }

        public void SetAllLicenseInformation (string licenseInformation)
        {
            lock (this.SyncRoot)
            {
                this.CachedLicenseInformation = licenseInformation;
                this.UpdateLicenseInformation();
            }
        }

        public void SetAllStatus (string statusText)
        {
            lock (this.SyncRoot)
            {
                this.CachedStatusText = statusText;
                this.UpdateStatusText();
            }
        }

        public void SetFooter (string splashScreenName, string footerText)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.SetFooter(footerText);
                    }
                }
            }
        }

        public void SetHideMouseCursor (string splashScreenName, bool hideMouseCursor)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.SetHideMouseCursor(hideMouseCursor);
                    }
                }
            }
        }

        public void SetLicenseInformation (string splashScreenName, string licenseInformation)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.SetLicenseInformation(licenseInformation);
                    }
                }
            }
        }

        public void SetStatus (string splashScreenName, string statusText)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.SetStatus(statusText);
                    }
                }
            }
        }

        public void Show (string splashScreenName)
        {
            lock (this.SyncRoot)
            {
                ISplashScreen[] splashScreens = this.GetSplashScreens(splashScreenName);

                if (splashScreens != null)
                {
                    foreach (ISplashScreen splashScreen in splashScreens)
                    {
                        splashScreen.Show();
                    }
                }
            }
        }

        public void ShowAll ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ISplashScreen>[] splashScreens = this.SplashScreens.ToArray();
                foreach (Lazy<ISplashScreen> splashScreen in splashScreens)
                {
                    splashScreen.Value.Show();
                }
            }
        }

        #endregion
    }
}
