using System;

using Prism.Mef;




namespace Logger.Core.Hosting
{
    public class Bootstrapper : MefBootstrapper
    {
        #region Instance Methods

        public new int Run (bool runWithDefaultConfiguration)
        {
            return Environment.ExitCode;
        }

        public new int Run ()
        {
            return this.Run(true);
        }

        #endregion
    }
}
