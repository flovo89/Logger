using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using Logger.Common.IO.Files;
using Logger.Common.ObjectModel;




namespace Logger.Core.Interfaces
{
    public interface ISessionManager : ISynchronizable
    {
        Application Application { get; }

        string ApplicationCompany { get; }

        string ApplicationCopyright { get; }

        Icon ApplicationIcon { get; }

        Guid ApplicationId { get; }

        string ApplicationName { get; }

        Version ApplicationVersion { get; }

        AggregateCatalog Catalog { get; }

        CompositionContainer Container { get; }

        DirectoryPath DataFolder { get; }

        bool DebugMode { get; }

        Dispatcher Dispatcher { get; }

        DirectoryPath ExecutableFolder { get; }

        IDictionary<string, IList<string>> InitialSettings { get; }

        Guid SessionId { get; }

        IEnumerable<string> SessionMode { get; }

        string SplashScreenThreadName { get; }

        ThreadPriority SplashScreenThreadPriority { get; }

        CultureInfo StartupFormattingCulture { get; }

        DateTime StartupTimestamp { get; }

        CultureInfo StartupUiCulture { get; }

        DirectoryPath TemporaryFolder { get; }

        bool UserInteractive { get; }

        void SetFormattingCulture (CultureInfo formattingCulture);

        void SetUiCulture (CultureInfo uiCulture);

        void Shutdown (int exitCode);
    }
}
