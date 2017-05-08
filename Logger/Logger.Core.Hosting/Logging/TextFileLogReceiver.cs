using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

using Logger.Common.DataTypes;
using Logger.Common.IO.Files;
using Logger.Common.Runtime;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;
using Logger.Core.Interfaces.Messaging;
using Logger.Core.Messaging;
using Logger.Core.Resources;




namespace Logger.Core.Logging
{
    [Export (typeof(ILogReceiver))]
    [Export (typeof(IMessageReceiver))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class TextFileLogReceiver : ILogReceiver,
            IMessageReceiver,
            IDisposable
    {
        #region Constants

        private const string CompositionFileName = "Composition.txt";

        private const string InitialValuesFileName = "InitialValues.txt";

        private const string LogDirectoryName = "Logs";

        private const string LogFilesBackupStreamName = "Logs.zip";

        private const string MainLogFileName = "MainLog.txt";

        private const string MessageFileName = "Messages.txt";

        private const string SystemInformationFileName = "SystemInformation.txt";

        private const string SystemInformationThreadName = "SystemInformationThread";

        private const ThreadPriority SystemInformationThreadPriority = ThreadPriority.Lowest;

        private static readonly Encoding CompositionFileEncoding = Encoding.UTF8;

        private static readonly Encoding InitialValuesFileEncoding = Encoding.UTF8;

        private static readonly Guid LogFilesBackupId = new Guid("B85018FC-C08B-47C4-8304-B1CDC4038090");

        private static readonly Encoding MainLogFileEncoding = Encoding.UTF8;

        private static readonly Encoding MessageFileEncoding = Encoding.UTF8;

        private static readonly TimeSpan MinimumLogRetentionTime = TimeSpan.FromDays(1.1);

        private static readonly Encoding SystemInformationFileEncoding = Encoding.UTF8;

        #endregion




        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public TextFileLogReceiver (ISessionManager sessionManager)
        {
            if (sessionManager == null)
            {
                throw new ArgumentNullException(nameof(sessionManager));
            }

            this.SyncRoot = new object();

            this.MaxFirstLineHeaderLength = 0;

            DateTime timestamp = sessionManager.StartupTimestamp;

            this.RootLogDirectory = sessionManager.DataFolder.Append(new DirectoryPath(TextFileLogReceiver.LogDirectoryName));
            this.RootLogDirectory.Create();

            this.CurrentLogDirectory = this.RootLogDirectory.Append(new DirectoryPath(timestamp.ToTechnical('-')));
            this.CurrentLogDirectory.Create();

            this.MainLogEncoding = TextFileLogReceiver.MainLogFileEncoding;
            this.MessageEncoding = TextFileLogReceiver.MessageFileEncoding;
            this.InitialValuesEncoding = TextFileLogReceiver.InitialValuesFileEncoding;
            this.CompositionEncoding = TextFileLogReceiver.CompositionFileEncoding;
            this.SystemInformationEncoding = TextFileLogReceiver.SystemInformationFileEncoding;

            this.BackupId = TextFileLogReceiver.LogFilesBackupId;
            this.BackupStreamName = TextFileLogReceiver.LogFilesBackupStreamName;

            string logStartMessage = string.Format(CultureInfo.InvariantCulture, "LOG START; LT: {0}; UTC: {1}", timestamp.ToTechnical('-'), timestamp.ToUniversalTime().ToTechnical('-'));

            this.MessageFile = null;
            this.CompositionFile = null;
            this.SystemInformationFile = null;

            this.MainLogFile = this.CurrentLogDirectory.Append(new FilePath(TextFileLogReceiver.MainLogFileName));
            this.MainLogFile.Directory.Create();

            this.MainLogWriter = new StreamWriter(this.MainLogFile.Path, false, this.MainLogEncoding);
            this.MainLogWriter.WriteLine(logStartMessage);
            this.MainLogWriter.Flush();

            this.InitialValuesFile = this.CurrentLogDirectory.Append(new FilePath(TextFileLogReceiver.InitialValuesFileName));
            this.InitialValuesFile.Directory.Create();

            this.WriteInitialValues(true, sessionManager);

            sessionManager.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Action(() => this.WriteInitialValues(false, this.SessionManager.Value)));

            if (sessionManager.DebugMode)
            {
                this.MessageFile = this.CurrentLogDirectory.Append(new FilePath(TextFileLogReceiver.MessageFileName));
                this.MessageFile.Directory.Create();

                this.MessageLogWriter = new StreamWriter(this.MessageFile.Path, false, this.MessageEncoding);
                this.MessageLogWriter.WriteLine(logStartMessage);
                this.MessageLogWriter.Flush();

                this.CompositionFile = this.CurrentLogDirectory.Append(new FilePath(TextFileLogReceiver.CompositionFileName));
                this.CompositionFile.Directory.Create();

                this.WriteCompositionValues(sessionManager);

                if (!Debugger.IsAttached)
                {
                    this.SystemInformationFile = this.CurrentLogDirectory.Append(new FilePath(TextFileLogReceiver.SystemInformationFileName));
                    this.SystemInformationFile.Directory.Create();

                    this.SystemInformationThread = new Thread(() =>
                    {
                        string systemInformation = SystemInformation.CreateReport(true);
                        this.SystemInformationFile.WriteText(systemInformation, false, this.SystemInformationEncoding);
                    });
                    this.SystemInformationThread.IsBackground = true;
                    this.SystemInformationThread.Name = TextFileLogReceiver.SystemInformationThreadName;
                    this.SystemInformationThread.Priority = TextFileLogReceiver.SystemInformationThreadPriority;
                    this.SystemInformationThread.SetApartmentState(ApartmentState.STA);
                    this.SystemInformationThread.Start();
                }
            }
        }

        ~TextFileLogReceiver ()
        {
            this.Dispose();
        }

        #endregion




        #region Instance Properties/Indexer

        public Guid BackupId { get; private set; }

        public string BackupStreamName { get; private set; }

        public Encoding CompositionEncoding { get; }

        public FilePath CompositionFile { get; }

        public DirectoryPath CurrentLogDirectory { get; }

        public Encoding InitialValuesEncoding { get; }

        public FilePath InitialValuesFile { get; }

        public Encoding MainLogEncoding { get; }

        public FilePath MainLogFile { get; }

        public Encoding MessageEncoding { get; }

        public FilePath MessageFile { get; }

        public DirectoryPath RootLogDirectory { get; }

        public Encoding SystemInformationEncoding { get; }

        public FilePath SystemInformationFile { get; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ILogManager> LogManager { get; private set; }

        [Import (typeof(IResourceManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<IResourceManager> ResourceManager { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ISessionManager> SessionManager { get; private set; }

        [Import (typeof(IShellManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<IShellManager> ShellManager { get; private set; }

        private StreamWriter MainLogWriter { get; set; }

        private int MaxFirstLineHeaderLength { get; set; }

        private StreamWriter MessageLogWriter { get; }

        private Thread SystemInformationThread { get; set; }

        #endregion




        #region Instance Methods

        private void WriteCompositionValues (ISessionManager sessionManager)
        {
            ReadOnlyCollection<ExportProvider> compositionProviders = sessionManager.Container.Providers;
            IQueryable<ComposablePartDefinition> compositionParts = sessionManager.Container.Catalog.Parts;

            using (StreamWriter compositionWriter = new StreamWriter(this.CompositionFile.Path, false, this.CompositionEncoding))
            {
                foreach (ExportProvider compositionProvider in compositionProviders)
                {
                    compositionWriter.WriteLine("Provider: " + compositionProvider);
                }

                compositionWriter.WriteLine();

                foreach (ComposablePartDefinition compositionPart in compositionParts)
                {
                    compositionWriter.WriteLine("----------------------------------------");
                    compositionWriter.WriteLine("PART");
                    compositionWriter.WriteLine(" - Contract name: " + compositionPart);
                    foreach (KeyValuePair<string, object> metadata in compositionPart.Metadata)
                    {
                        compositionWriter.WriteLine(" - Meta data:     " + ( metadata.Key ?? "[null]" ) + "=" + ( metadata.Value ?? "[null]" ));
                    }
                    foreach (ExportDefinition export in compositionPart.ExportDefinitions)
                    {
                        compositionWriter.WriteLine("EXPORT");
                        compositionWriter.WriteLine(" - Contract name: " + export.ContractName);
                        foreach (KeyValuePair<string, object> metadata in export.Metadata)
                        {
                            compositionWriter.WriteLine(" - Meta data:     " + ( metadata.Key ?? "[null]" ) + "=" + ( metadata.Value ?? "[null]" ));
                        }
                    }
                    foreach (ImportDefinition import in compositionPart.ImportDefinitions)
                    {
                        compositionWriter.WriteLine("IMPORT");
                        compositionWriter.WriteLine(" - Contract name:   " + import.ContractName);
                        compositionWriter.WriteLine(" - Cardinality:     " + import.Cardinality);
                        compositionWriter.WriteLine(" - Constraint:      " + import.Constraint);
                        compositionWriter.WriteLine(" - Is prerequisite: " + import.IsPrerequisite);
                        compositionWriter.WriteLine(" - Is recomposable: " + import.IsRecomposable);
                    }
                    compositionWriter.WriteLine("----------------------------------------");
                    compositionWriter.WriteLine();
                }

                compositionWriter.Flush();
            }
        }

        private void WriteInitialValues (bool sessionManagerOnly, ISessionManager sessionManager)
        {
            IDictionary<string, IList<string>> initialSettings = sessionManager.InitialSettings;

            IResourceSet[] resourceSets = sessionManagerOnly ? null : this.ResourceManager.Value.GetAvailableResourceSets();
            DirectoryPath[] logDirectories = sessionManagerOnly ? null : this.LogManager.Value.GetCommonLogDirectory().FindAllDirectories(false);

            Application application = sessionManagerOnly ? null : Application.Current;

            using (StreamWriter initialValuesWriter = new StreamWriter(this.InitialValuesFile.Path, false, this.InitialValuesEncoding))
            {
                if (initialSettings != null)
                {
                    initialValuesWriter.WriteLine("[InitialSettings]");
                    foreach (KeyValuePair<string, IList<string>> initialSetting in initialSettings)
                    {
                        foreach (string value in initialSetting.Value)
                        {
                            initialValuesWriter.WriteLine(initialSetting.Key + "=" + value);
                        }
                    }
                    initialValuesWriter.WriteLine();
                }


                if (resourceSets != null)
                {
                    initialValuesWriter.WriteLine("[ResourceSets]");
                    foreach (IResourceSet resourceSet in resourceSets)
                    {
                        initialValuesWriter.WriteLine(resourceSet.Key);
                    }
                    initialValuesWriter.WriteLine();
                }


                if (logDirectories != null)
                {
                    initialValuesWriter.WriteLine("[LogDirectories]");
                    foreach (DirectoryPath logDirectory in logDirectories)
                    {
                        initialValuesWriter.WriteLine(logDirectory);
                    }
                    initialValuesWriter.WriteLine();
                }

                if (application != null)
                {
                    initialValuesWriter.WriteLine("[ApplicationProperties]");
                    foreach (DictionaryEntry property in application.Properties)
                    {
                        initialValuesWriter.WriteLine(property.Key + "=" + property.Value);
                    }
                    initialValuesWriter.WriteLine();

                    initialValuesWriter.WriteLine("[ApplicationResources]");
                    foreach (DictionaryEntry resource in application.Resources)
                    {
                        initialValuesWriter.WriteLine(resource.Key + "=" + resource.Value);
                    }
                    initialValuesWriter.WriteLine();
                }

                initialValuesWriter.Flush();
            }
        }

        #endregion




        #region Interface: IDisposable

        public void Dispose ()
        {
            lock (this.SyncRoot)
            {
                if (this.SystemInformationThread != null)
                {
                    try
                    {
                        this.SystemInformationThread.Abort();
                    }
                    catch
                    {
                    }

                    this.SystemInformationThread = null;
                }

                if (this.MainLogWriter != null)
                {
                    try
                    {
                        this.MainLogWriter.Flush();
                    }
                    catch
                    {
                    }

                    try
                    {
                        this.MainLogWriter.Close();
                    }
                    catch
                    {
                    }

                    this.MainLogWriter = null;
                }

                if (this.MessageLogWriter != null)
                {
                    try
                    {
                        this.MessageLogWriter.Flush();
                    }
                    catch
                    {
                    }

                    try
                    {
                        this.MessageLogWriter.Close();
                    }
                    catch
                    {
                    }

                    this.MainLogWriter = null;
                }
            }
        }

        #endregion




        #region Interface: ILogReceiver

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        public void CleanupLog (DateTime retentionDate)
        {
            lock (this.SyncRoot)
            {
                DateTime minimumRetentionDate = this.SessionManager.Value.StartupTimestamp.Subtract(TextFileLogReceiver.MinimumLogRetentionTime);
                DirectoryPath[] logDirectories = this.RootLogDirectory.FindAllDirectories(false);
                foreach (DirectoryPath logDirectory in logDirectories)
                {
                    if (this.CurrentLogDirectory.Equals(logDirectory))
                    {
                        continue;
                    }

                    if (logDirectory.DirectoryName.IsDateTime(CultureInfo.InvariantCulture))
                    {
                        DateTime timestamp = logDirectory.DirectoryName.ToDateTime(CultureInfo.InvariantCulture);
                        if (( retentionDate > timestamp ) && ( minimumRetentionDate > timestamp ))
                        {
                            this.LogManager.Value.Log(this.GetType().Name, LogLevel.Information, "Deleting old log directory: {0}.", logDirectory);
                            logDirectory.TryDelete();
                        }
                    }
                }
            }
        }

        public DirectoryPath GetCommonLogDirectory ()
        {
            lock (this.SyncRoot)
            {
                return this.RootLogDirectory;
            }
        }

        public DirectoryPath GetCurrentLogDirectory ()
        {
            lock (this.SyncRoot)
            {
                return this.CurrentLogDirectory;
            }
        }

        public void Log (string source, LogLevel level, string message, DateTime timestamp, int threadId, params object[] args)
        {
            lock (this.SyncRoot)
            {
                if (this.MainLogWriter != null)
                {
                    try
                    {
                        string actualMessage = string.Format(CultureInfo.InvariantCulture, message, args);

                        int newLineIndex = actualMessage.IndexOf('\n');
                        string firstLine = newLineIndex == -1 ? actualMessage : actualMessage.Substring(0, newLineIndex).Trim();
                        string[] subsequentLines = newLineIndex == -1 ? null : actualMessage.Substring(newLineIndex + 1).Trim().SplitLineBreaks(StringSplitOptions.None);

                        string firstLineHeader = "# " + timestamp.ToTechnical('-') + " [" + threadId.ToString("D3", CultureInfo.InvariantCulture) + "] " + ( "[" + level + "]" ).PadRight(13, ' ') + " " + ( "[" + source + "]" ).PadRight(25, ' ') + " ";

                        if (this.MaxFirstLineHeaderLength < firstLineHeader.Length)
                        {
                            this.MaxFirstLineHeaderLength = firstLineHeader.Length;
                        }

                        this.MainLogWriter.WriteLine(firstLineHeader.PadRight(this.MaxFirstLineHeaderLength, ' ') + firstLine.Trim());

                        if (subsequentLines != null)
                        {
                            foreach (string subsequentLine in subsequentLines)
                            {
                                if (!subsequentLine.IsEmpty())
                                {
                                    this.MainLogWriter.WriteLine(">".PadRight(this.MaxFirstLineHeaderLength, ' ') + subsequentLine);
                                }
                            }
                        }

                        this.MainLogWriter.Flush();
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion




        #region Interface: IMessageReceiver

        public void ReceiveMessage (IMessage message)
        {
            lock (this.SyncRoot)
            {
                if (this.MessageLogWriter != null)
                {
                    try
                    {
                        this.MessageLogWriter.WriteLine("----------------------------------------");
                        this.MessageLogWriter.WriteLine(message.Timestamp.ToTechnical('-'));
                        this.MessageLogWriter.WriteLine("----------------------------------------");
                        this.MessageLogWriter.WriteLine(" - Name:         " + message.Name);
                        this.MessageLogWriter.WriteLine(" - ID:           " + message.Id.ToString("N").ToUpperInvariant());
                        this.MessageLogWriter.WriteLine(" - Serializable: " + message.IsSerializable);
                        foreach (KeyValuePair<string, object> data in message.Data)
                        {
                            this.MessageLogWriter.WriteLine(" - Data type:    " + ( data.Key ?? "[null]" ) + "=" + ( data.Value == null ? "[null]" : data.Value.GetType().FullName ));
                            this.MessageLogWriter.WriteLine(" - Data value:   " + ( data.Key ?? "[null]" ) + "=" + ( data.Value ?? "[null]" ));
                        }
                        this.MessageLogWriter.WriteLine("----------------------------------------");
                        this.MessageLogWriter.WriteLine();
                        this.MessageLogWriter.Flush();
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion
    }
}
