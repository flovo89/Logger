using System.ComponentModel.Composition;

using Logger.Core;
using Logger.Core.Interfaces;

using Microsoft.Practices.Prism.Commands;




namespace Logger.Modules.Logger
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
