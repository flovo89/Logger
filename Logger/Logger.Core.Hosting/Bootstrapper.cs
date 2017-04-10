using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using Logger.Common.Base.Collections.Generic;
using Logger.Common.Base.DataTypes;
using Logger.Common.Base.IO.Files;
using Logger.Common.Base.IO.Keyboard;
using Logger.Common.Base.Reflection;
using Logger.Common.Base.Runtime;
using Logger.Common.Base.Threading;
using Logger.Common.Base.User;
using Logger.Core.Hosting.Properties;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Resources;
using Logger.Core.Interfaces.Settings;

using Prism.Mef;




namespace Logger.Core.Hosting
{
    public class Bootstrapper : MefBootstrapper
    {
        #region Constants

        private const bool DefaultAllowMultipleInstances = false;

        private const Environment.SpecialFolder DefaultDataDirectoryFolder = Environment.SpecialFolder.LocalApplicationData;

        private const string DefaultPrimaryThreadName = "PrimaryThread";

        private const ShutdownMode DefaultShutdownMode = ShutdownMode.OnExplicitShutdown;

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

        public bool AllowMultipleInstances { get; private set; }

        public Application Application { get; private set; }

        public Assembly ApplicationAssembly { get; private set; }

        public string ApplicationCompany { get; private set; }

        public string ApplicationCopyright { get; private set; }

        public DirectoryPath ApplicationDirectory { get; private set; }

        public Icon ApplicationIcon { get; private set; }

        public Guid ApplicationId { get; private set; }

        public string ApplicationName { get; private set; }

        public Version ApplicationVersion { get; private set; }

        public AggregateCatalog Catalog { get; private set; }

        public new CompositionContainer Container
        {
            get
            {
                return base.Container;
            }
        }

        public DirectoryPath DataDirectory { get; private set; }

        public bool DebugMode { get; private set; }

        public bool HasMultipleInstances { get; private set; }

        public CultureInfo InitialFormattingCulture { get; private set; }

        public IDictionary<string, IList<string>> InitialSettings { get; private set; }

        public CultureInfo InitialUiCulture { get; private set; }

        public Thread PrimaryThread { get; private set; }

        public Process Process { get; private set; }

        public Guid SessionId { get; private set; }

        public ISet<string> SessionMode { get; private set; }

        public string SplashScreenThreadName { get; private set; }

        public ThreadPriority SplashScreenThreadPriority { get; private set; }

        public DirectoryPath TemporaryDirectory { get; private set; }

        public bool UserInteractive { get; private set; }

        private bool IsRunning { get; set; }

        private bool IsShuttingDown { get; }

        private Mutex SessionMutex { get; set; }

        private bool SessionMutexCreated { get; set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(IResourceManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<IResourceManager> ResourceManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISessionManager> SessionManager { get; private set; }

        [Import (typeof(ISettingManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        protected internal Lazy<ISettingManager> SettingManager { get; private set; }

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

                this.InitialSettings = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);
                this.GetInitialSettings(this.InitialSettings);

                this.InitialUiCulture = CultureInfo.CurrentUICulture;
                this.InitialFormattingCulture = CultureInfo.CurrentCulture;

                this.SessionId = Guid.NewGuid();

                this.SessionMode = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                this.GetSessionMode(this.SessionMode);

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

                this.ApplicationVersion = this.GetApplicationVersion();
                if (this.ApplicationVersion == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationVersionIsRequired);
                }

                this.ApplicationId = this.GetApplicationId();

                this.UserInteractive = this.GetUserInteractive();
                this.DebugMode = this.GetDebugMode();

                if (!WindowsVersion.IsRunningWindows7OrNewer)
                {
                    if (this.UserInteractive)
                    {
                        MessageBox.Show(Resources.Bootstrapper_InvalidOSVersion, this.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
                    }
                    Environment.ExitCode = SessionExitCodes.SystemRequirementsNotFulfilled;
                    return Environment.ExitCode;
                }

                this.GetSessionMode(this.SessionMode);

                this.AllowMultipleInstances = this.GetAllowMultipleInstances();
                this.HasMultipleInstances = this.GetHasMultipleInstances();

                if (!this.AllowMultipleInstances && this.HasMultipleInstances)
                {
                    if (this.UserInteractive)
                    {
                        MessageBox.Show(string.Format(Resources.Bootstrapper_AnotherInstanceIsAlreadyRunning, this.ApplicationName), this.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.None);
                    }
                    Environment.ExitCode = SessionExitCodes.AnotherInstanceIsAlreadyRunning;
                    return Environment.ExitCode;
                }

                this.ApplicationDirectory = this.GetApplicationDirectory();
                if (this.ApplicationDirectory == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationDirectoryIsRequired);
                }

                this.DataDirectory = this.GetDataDirectory();
                if (this.DataDirectory == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_DataDirectoryIsRequired);
                }

                this.TemporaryDirectory = this.GetTemporaryDirectory();
                if (this.TemporaryDirectory == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_TemporaryDirectoryIsRequired);
                }

                this.ApplicationDirectory.Create();
                this.DataDirectory.Create();
                this.TemporaryDirectory.Create();

                Environment.CurrentDirectory = this.DataDirectory.Path;

                this.Application = this.CreateApplication();
                if (this.Application == null)
                {
                    throw new InvalidOperationException(Resources.Bootstrapper_ApplicationInstanceIsRequired);
                }

                this.Application.ShutdownMode = this.GetShutdownMode();
                this.Application.Exit += (sender, e) =>
                {
                    this.SessionManager.Value.Shutdown(e.ApplicationExitCode);
                    this.Application.Dispatcher.DoEvents();
                };

                //Bootstrapper sequence:
                // 1. CreateLogger()
                // 2. CreateModuleCatalog()
                // 3. ConfigureModuleCatalog()
                // 4. CreateAggregateCatalog()
                // 5. ConfigureAggregateCatalog()
                // 6. RegisterDefaultTypesIfMissing()
                // 7. CreateContainer()
                // 8. ConfigureContainer()
                // 9. RegisterBootstrapperProvidedTypes
                //10. ConfigureServiceLocator()
                //11. ConfigureRegionAdapterMappings()
                //12. ConfigureDefaultRegionBehaviors()
                //13. RegisterFrameworkExceptionTypes()
                //14. CreateShell()
                //15. InitializeShell()
                //16. InitializeModules()
                base.Run(runWithDefaultConfiguration);

                this.SettingManager.Value.Load();
                this.ResourceManager.Value.Load();

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "-------------------- BOOTSTRAPPING COMPLETED --------------------");

                this.SessionManager.Value.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() =>
                {
                }));

                this.LogStartup();

                int exitCode = this.Application.Run();

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "-------------------- OPERATIONS COMPLETED --------------------");

                this.ResourceManager.Value.Save();
                this.SettingManager.Value.Save();

                this.SessionManager.Value.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                }));

                switch (exitCode)
                {
                    case SessionExitCodes.LogoffSystem:
                    {
                        this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "Logging off system");
                        WindowsSession.Logoff(false);
                        break;
                    }

                    case SessionExitCodes.ShutdownSystem:
                    {
                        this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "Shutting down system");
                        WindowsSession.Shutdown(false);
                        break;
                    }

                    case SessionExitCodes.RestartSystem:
                    {
                        this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "Restarting system");
                        WindowsSession.Restart(false);
                        break;
                    }
                }

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "-------------------- SHUTDOWN COMPLETED --------------------");

                Environment.ExitCode = exitCode;
                return Environment.ExitCode;
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

        protected virtual Application CreateApplication ()
        {
            return Application.Current ?? new Application();
        }

        protected virtual bool GetAllowMultipleInstances ()
        {
            return Bootstrapper.DefaultAllowMultipleInstances;
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

        protected virtual DirectoryPath GetApplicationDirectory ()
        {
            return this.ApplicationAssembly.GetLocation();
        }

        protected virtual Icon GetApplicationIcon ()
        {
            return this.ApplicationAssembly.GetDefaultIcon();
        }

        protected virtual Guid GetApplicationId ()
        {
            Guid? id = this.ApplicationAssembly.GetGuid(AssemblyGuidFlags.UseAssemblyName | AssemblyGuidFlags.IgnoreVersion);
            return id.HasValue ? id.Value : Guid.Empty;
        }

        protected virtual string GetApplicationName ()
        {
            return this.ApplicationAssembly.GetProductName();
        }

        protected virtual Version GetApplicationVersion ()
        {
            return this.ApplicationAssembly.GetProductVersion();
        }

        protected virtual DirectoryPath GetDataDirectory ()
        {
            Environment.SpecialFolder folder = Bootstrapper.DefaultDataDirectoryFolder;

            if (this.InitialSettings.ContainsKey(SettingNames.DataDirectorySpecialFolder))
            {
                foreach (string value in this.InitialSettings[SettingNames.DataDirectorySpecialFolder])
                {
                    if (value.IsEnumeration(typeof(Environment.SpecialFolder)))
                    {
                        folder = (Environment.SpecialFolder)value.ToEnumeration(typeof(Environment.SpecialFolder));
                        break;
                    }
                }
            }

            DirectoryPath resolvedFolder = new DirectoryPath(Environment.GetFolderPath(folder, Environment.SpecialFolderOption.None));

            DirectoryPath path = null;

            if (this.InitialSettings.ContainsKey(SettingNames.DataDirectoryPath))
            {
                foreach (string value in this.InitialSettings[SettingNames.DataDirectoryPath])
                {
                    if (value.IsDirectoryPath() && !value.IsEmpty())
                    {
                        path = value.ToDirectoryPath();
                        break;
                    }
                }
            }

            if (path != null)
            {
                if (path.IsAbsolute)
                {
                    return path;
                }

                path = path.ToAbsolutePath(resolvedFolder);
            }
            else
            {
                path = resolvedFolder;
            }


            return path.Append(new DirectoryPath(this.ApplicationCompany.ToPathCompatible(true))).Append(new DirectoryPath(this.ApplicationName.ToPathCompatible(true)));
        }

        protected virtual bool GetDebugMode ()
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

            if (SystemKeyboard.IsKeyPressed(SystemKeyboardKey.LShift) || SystemKeyboard.IsKeyPressed(SystemKeyboardKey.RShift))
            {
                return true;
            }

            if (this.InitialSettings.ContainsKey(SettingNames.DebugMode))
            {
                if (this.InitialSettings[SettingNames.DebugMode].Count > 0)
                {
                    if (this.InitialSettings[SettingNames.DebugMode][0].IsBoolean())
                    {
                        return this.InitialSettings[SettingNames.DebugMode][0].ToBoolean();
                    }
                }
            }

            return false;
        }

        protected virtual bool GetHasMultipleInstances ()
        {
            bool sessionMutexCreated = false;
            this.SessionMutexCreated = true;
            this.SessionMutex = new Mutex(true, this.ApplicationId.ToString("N", CultureInfo.InvariantCulture), out sessionMutexCreated);
            this.SessionMutexCreated = sessionMutexCreated;
            return !sessionMutexCreated;
        }

        protected virtual void GetInitialSettings (IDictionary<string, IList<string>> initialSettings)
        {
            IDictionary<string, IList<string>> commandLineParameters = null;
            CommandLine.Parse(out commandLineParameters);
            foreach (KeyValuePair<string, IList<string>> commandLineParameter in commandLineParameters)
            {
                if (!initialSettings.ContainsKey(commandLineParameter.Key))
                {
                    initialSettings.Add(commandLineParameter.Key, new List<string>());
                }

                initialSettings[commandLineParameter.Key].AddRange(commandLineParameter.Value);
            }

            foreach (string appSettingKey in ConfigurationManager.AppSettings)
            {
                string[] appSettingValues = ConfigurationManager.AppSettings.GetValues(appSettingKey);

                if (!initialSettings.ContainsKey(appSettingKey))
                {
                    initialSettings.Add(appSettingKey, new List<string>());
                }

                initialSettings[appSettingKey].AddRange(appSettingValues);
            }

            /*IDictionary<string, IList<string>> environmentVariables = EnvironmentVariables.GetAllEnvironmentVariables();
			foreach (KeyValuePair<string, IList<string>> environmentVariable in environmentVariables)
			{
				if (!initialSettings.ContainsKey(environmentVariable.Key))
				{
					initialSettings.Add(environmentVariable.Key, new List<string>());
				}

				initialSettings[environmentVariable.Key].AddRange(environmentVariable.Value);
			}*/
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

        protected virtual void GetSessionMode (ISet<string> sessionMode)
        {
            if (this.ApplicationAssembly == null)
            {
                return;
            }

            SessionModeAttribute[] attributes = this.ApplicationAssembly.GetCustomAttributes(typeof(SessionModeAttribute), true).OfType<SessionModeAttribute>().ToArray();
            sessionMode.AddRange(from x in attributes select x.SessionMode);
        }

        protected virtual ShutdownMode GetShutdownMode ()
        {
            return Bootstrapper.DefaultShutdownMode;
        }

        protected virtual string GetSplashScreenThreadName ()
        {
            return Bootstrapper.DefaultSplashScreenThreadName;
        }

        protected virtual ThreadPriority GetSplashScreenThreadPriority ()
        {
            return Bootstrapper.DefaultSplashScreenThreadPriority;
        }

        protected virtual DirectoryPath GetTemporaryDirectory ()
        {
            DirectoryPath path = null;

            if (this.InitialSettings.ContainsKey(SettingNames.TemporaryDirectoryPath))
            {
                foreach (string value in this.InitialSettings[SettingNames.TemporaryDirectoryPath])
                {
                    if (value.IsDirectoryPath() && !value.IsEmpty())
                    {
                        DirectoryPath pathCandidate = value.ToDirectoryPath();
                        if (pathCandidate.IsAbsolute)
                        {
                            path = pathCandidate;
                            break;
                        }
                    }
                }
            }

            if (path == null)
            {
                path = DirectoryPath.GetTempDirectory().Append(this.ApplicationName.ToPathCompatible(false).ToDirectoryPath());
            }

            path.TryDelete();
            path.Create();

            return path;
        }

        protected virtual bool GetUserInteractive ()
        {
            if (this.InitialSettings.ContainsKey(SettingNames.UserInteractive))
            {
                if (this.InitialSettings[SettingNames.UserInteractive].Count > 0)
                {
                    if (this.InitialSettings[SettingNames.UserInteractive][0].IsBoolean())
                    {
                        return this.InitialSettings[SettingNames.UserInteractive][0].ToBoolean();
                    }
                }
            }

            return Environment.UserInteractive;
        }

        protected virtual void LogStartup ()
        {
            try
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application name:              {0}", this.ApplicationName);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application version:           {0}", this.ApplicationVersion.ToString(4));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application company:           {0}", this.ApplicationCompany);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application copyright:         {0}", this.ApplicationCopyright);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application assembly:          {0}", this.ApplicationAssembly.FullName);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application directory:         {0}", this.ApplicationDirectory);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application directory size:    {0}", this.ApplicationDirectory.Size);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Application ID:                {0}", this.ApplicationId.ToString("N"));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Allow multiple instances:      {0}", this.AllowMultipleInstances);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Has multiple instances:        {0}", this.HasMultipleInstances);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Data directory:                {0}", this.DataDirectory);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Data directory size:           {0}", this.DataDirectory.Size);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Temporary directory:           {0}", this.TemporaryDirectory);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Initial formatting culture:    {0}", this.InitialFormattingCulture);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Initial UI culture:            {0}", this.InitialUiCulture);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Installed UI culture:          {0}", CultureInfo.InstalledUICulture);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Current formatting culture:    {0}", CultureInfo.CurrentCulture);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Current UI culture:            {0}", CultureInfo.CurrentUICulture);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Primary thread ID:             {0}", this.PrimaryThread.ManagedThreadId);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Primary thread priority:       {0}", this.PrimaryThread.Priority);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Primary thread name:           {0}", this.PrimaryThread.Name);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Process ID:                    {0}", this.Process.Id);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Process priority:              {0}", this.Process.PriorityClass);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Splash screen thread priority: {0}", this.SplashScreenThreadPriority);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Splash screen thread name:     {0}", this.SplashScreenThreadName);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Main window:                   {0}", this.Application.MainWindow.GetType().FullName);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Startup timestamp:             {0}", this.Process.StartTime.ToTechnical('-'));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Session ID:                    {0}", this.SessionId.ToString("N"));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Interactive session:           {0}", this.UserInteractive);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Debug mode:                    {0}", this.DebugMode);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Shutdown mode:                 {0}", this.Application.ShutdownMode);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Computer:                      {0}", SystemUsers.NetworkDomain + "\\" + SystemUsers.LocalDomain);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "User:                          {0}", SystemUsers.CurrentDomain + "\\" + SystemUsers.CurrentUser);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "64 bit process:                {0}", Environment.Is64BitProcess);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "64 bit OS:                     {0}", Environment.Is64BitOperatingSystem);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "OS version:                    {0}", Environment.OSVersion.ToString());
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Default encoding:              {0}", Encoding.Default.EncodingName);
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Machine ID:                    {0}", UniqueIdentification.GetLocalMachineId().ToString("N"));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Domain ID:                     {0}", UniqueIdentification.GetNetworkDomainId().ToString("N"));
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Current log folder:            {0}", this.LogManager.Value.GetCurrentLogDirectory());
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Common log folder:             {0}", this.LogManager.Value.GetCommonLogDirectory());
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Common log folder size:        {0}", this.LogManager.Value.GetCommonLogDirectory().Size);

                foreach (string sessionMode in this.SessionMode)
                {
                    this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Session mode:                  {0}.", sessionMode);
                }
            }
            catch (Exception exception)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Warning, "Startup log exception:{0}{1}", Environment.NewLine, exception.ToDetailedString(' '));
            }
        }

        #endregion
    }
}
