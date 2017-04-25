using System.ComponentModel;
using System.ComponentModel.Composition;

using Logger.Core;
using Logger.Core.Interfaces;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Modules.Logger
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
        public LoggerMainViewShell (IRegionManager regionManager)
        {
            this.InitializeComponent();

            //RegionManager.SetRegionManager(this.ViewsRibbonTab, regionManager);
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
