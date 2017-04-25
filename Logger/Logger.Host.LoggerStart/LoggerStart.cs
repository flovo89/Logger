using System;

using Logger.Core;
using Logger.Core.Interfaces;
using Logger.Modules.Logger;




[assembly: SessionMode (SessionModeNames.Logger)]




namespace Logger.Host
{
    public static class LoggerStart
    {
        #region Static Methods

        [STAThread]
        public static int Main ()
        {
            Bootstrapper bootstrapper = new Bootstrapper();
            return bootstrapper.Run();
        }

        #endregion
    }
}
