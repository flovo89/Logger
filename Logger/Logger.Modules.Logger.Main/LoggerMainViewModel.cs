using System.ComponentModel.Composition;

using Logger.Core.Interfaces;
using Logger.Core.Modularity;
using Logger.Modules.Logger.Core;

using Microsoft.Practices.Prism.Commands;




namespace Logger.Modules.Logger.Main
{
    [Export (nameof(LoggerMainViewModel))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    [PartMetadata (PartMetadataNames.SessionMode, SessionModeNames.Logger)]
    public sealed class LoggerMainViewModel : ViewModel
    {
        #region Instance Properties/Indexer

        public DelegateCommand ExitCommand { get; private set; }

        #endregion




        #region Instance Methods

        private void Exit ()
        {
        }

        #endregion




        #region Overrides

        protected override void Initialize ()
        {
            this.ExitCommand = new DelegateCommand(this.Exit);
        }

        #endregion
    }
}
