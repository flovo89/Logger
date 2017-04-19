using System.ComponentModel;
using System.ComponentModel.Composition;

using Logger.Core.Interfaces;
using Logger.Core.Modularity;
using Logger.Modules.Logger.Core;




namespace Logger.Modules.Logger.Main
{
    [Export (LoggerMainViewShell.ViewName)]
    [PartCreationPolicy (CreationPolicy.Shared)]
    [PartMetadata (PartMetadataNames.SessionMode, SessionModeNames.Logger)]
    [MainWindow]
    public partial class LoggerMainViewShell : Shell
    {
        #region Constants

        internal const string ViewName = nameof(LoggerMainViewShell);

        #endregion




        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public LoggerMainViewShell ()
        {
            this.InitializeComponent();
        }

        #endregion




        #region Overrides

        protected override async void OnClosing (CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;

            await ( (LoggerMainViewModel)this.ViewModel ).ExitCommand.Execute();
        }

        #endregion
    }
}
