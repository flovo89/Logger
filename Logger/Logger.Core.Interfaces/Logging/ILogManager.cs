using System;

using Logger.Common.Base.IO.Files;
using Logger.Common.Base.ObjectModel;




namespace Logger.Core.Interfaces.Logging
{
    public interface ILogManager : ISynchronizable
    {
        void CleanupLog (DateTime retentionDate);

        void CleanupLog (TimeSpan retentionTime);

        DirectoryPath GetCommonLogDirectory ();

        DirectoryPath GetCurrentLogDirectory ();

        void Log (string source, LogLevel level, string message, params object[] args);

        void Log (string source, LogLevel level, string message, DateTime timestamp, int threadId, params object[] args);
    }
}
