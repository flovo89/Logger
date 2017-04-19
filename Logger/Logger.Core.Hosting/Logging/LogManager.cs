using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Logger.Common.Base.IO.Files;
using Logger.Common.Base.ObjectModel;
using Logger.Common.Base.Windows;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;




namespace Logger.Core.Hosting.Logging
{
    [Export (typeof(ILogManager))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class LogManager : ILogManager
    {
        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public LogManager ()
        {
            this.SyncRoot = new object();

            this.IsLogging = false;

            LogManagerTraceListener traceListener = Trace.Listeners.OfType<LogManagerTraceListener>().FirstOrDefault();
            bool alreadyAdded = traceListener != null;
            traceListener = traceListener ?? new LogManagerTraceListener(this);
            this.TraceListener = traceListener;
            if (!alreadyAdded)
            {
                Trace.Listeners.Add(traceListener);
            }
        }

        #endregion




        #region Instance Properties/Indexer

        [ImportMany (typeof(ILogReceiver), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<ILogReceiver>> Providers { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ISessionManager> SessionManager { get; private set; }

        private bool IsLogging { get; set; }

        private LogManagerTraceListener TraceListener { get; set; }

        #endregion




        #region Interface: ILogManager

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
                using (TemporaryCursor.Hourglass())
                {
                    Lazy<ILogReceiver>[] receivers = this.Providers.ToArray();
                    foreach (Lazy<ILogReceiver> provider in receivers)
                    {
                        provider.Value.CleanupLog(retentionDate);
                    }
                }
            }
        }

        public void CleanupLog (TimeSpan retentionTime)
        {
            this.CleanupLog(DateTime.Now.Subtract(retentionTime));
        }

        public DirectoryPath GetCommonLogDirectory ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ILogReceiver>[] receivers = this.Providers.ToArray();
                foreach (Lazy<ILogReceiver> provider in receivers)
                {
                    DirectoryPath logDirectory = provider.Value.GetCommonLogDirectory();
                    if (logDirectory != null)
                    {
                        return logDirectory;
                    }
                }
                return null;
            }
        }

        public DirectoryPath GetCurrentLogDirectory ()
        {
            lock (this.SyncRoot)
            {
                Lazy<ILogReceiver>[] receivers = this.Providers.ToArray();
                foreach (Lazy<ILogReceiver> provider in receivers)
                {
                    DirectoryPath logDirectory = provider.Value.GetCurrentLogDirectory();
                    if (logDirectory != null)
                    {
                        return logDirectory;
                    }
                }
                return null;
            }
        }

        public void Log (string source, LogLevel level, string message, params object[] args)
        {
            this.Log(source, level, message, DateTime.Now, Thread.CurrentThread.ManagedThreadId, args);
        }

        public void Log (string source, LogLevel level, string message, DateTime timestamp, int threadId, params object[] args)
        {
            lock (this.SyncRoot)
            {
                if (!this.IsLogging)
                {
                    try
                    {
                        this.IsLogging = true;

                        if (source != null)
                        {
                            source = source.IsEmpty() ? null : source.Trim();
                        }

                        if (message != null)
                        {
                            message = message.IsEmpty() ? null : message.Trim();
                        }

                        source = source ?? string.Empty;
                        message = message ?? string.Empty;
                        args = args ?? new object[0];

                        Lazy<ILogReceiver>[] receivers = this.Providers.ToArray();
                        foreach (Lazy<ILogReceiver> provider in receivers)
                        {
                            provider.Value.Log(source, level, message, timestamp, threadId, args);
                        }
                    }
                    finally
                    {
                        this.IsLogging = false;
                    }
                }
            }
        }

        #endregion




        #region Type: LogManagerTraceListener

        private sealed class LogManagerTraceListener : TraceListener,
                ISynchronizable
        {
            #region Instance Constructor/Destructor

            public LogManagerTraceListener (LogManager logManager)
            {
                if (logManager == null)
                {
                    throw new ArgumentNullException(nameof(logManager));
                }

                this.SyncRoot = new object();

                this.LogManager = logManager;

                this.MessageCache = new StringBuilder();
            }

            ~LogManagerTraceListener ()
            {
                this.Close();
            }

            #endregion




            #region Instance Properties/Indexer

            public LogManager LogManager { get; }

            private StringBuilder MessageCache { get; }

            #endregion




            #region Instance Methods

            private void FlushCache ()
            {
                while (true)
                {
                    string messageToWrite = this.MessageCache.ToString();

                    int newLineIndex = messageToWrite.IndexOf(Environment.NewLine, StringComparison.InvariantCultureIgnoreCase);
                    if (newLineIndex == -1)
                    {
                        break;
                    }

                    this.MessageCache.Clear();

                    try
                    {
                        lock (this.LogManager.SyncRoot)
                        {
                            if (this.LogManager.SessionManager.Value.DebugMode)
                            {
                                this.LogManager.Log("TRACE", LogLevel.Debug, "{0}", messageToWrite);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            #endregion




            #region Overrides

            public override bool IsThreadSafe
            {
                get
                {
                    return this.IsSynchronized;
                }
            }

            public override void Close ()
            {
                this.FlushCache();
                base.Close();
            }

            public override void Flush ()
            {
                this.FlushCache();
                base.Flush();
            }

            public override void Write (string message)
            {
                lock (this.SyncRoot)
                {
                    this.MessageCache.Append(message);
                    this.FlushCache();
                }
            }

            public override void WriteLine (string message)
            {
                lock (this.SyncRoot)
                {
                    this.MessageCache.Append(message);
                    this.MessageCache.Append(Environment.NewLine);
                    this.FlushCache();
                }
            }

            protected override void Dispose (bool disposing)
            {
                this.FlushCache();
                base.Dispose(disposing);
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

        #endregion
    }
}
