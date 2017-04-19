using System;

using Logger.Common.Base.IO.Files;
using Logger.Common.Base.ObjectModel;




namespace Logger.Core.Interfaces.Logging
{
    public interface ILogReceiver : ISynchronizable
    {
        void CleanupLog (DateTime retentionDate);

        DirectoryPath GetCommonLogDirectory ();

        DirectoryPath GetCurrentLogDirectory ();

        void Log (string source, LogLevel level, string message, DateTime timestamp, int threadId, object[] args);
    }
}
